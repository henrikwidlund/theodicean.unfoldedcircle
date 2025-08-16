using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Server.Event;
using UnfoldedCircle.Server.Json;
using UnfoldedCircle.Server.Response;

namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    /// <summary>
    /// Called when a <c>connect</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnConnect(ConnectEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>disconnect</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns><see langword="true"/> if disconnect was successful, otherwise <see langword="false"/>.</returns>
    protected abstract ValueTask<bool> OnDisconnect(DisconnectEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>abort_driver_setup</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnAbortDriverSetup(AbortDriverSetupEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>enter_standby</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnEnterStandby(EnterStandbyEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>exit_standby</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnExitStandby(ExitStandbyEvent payload, string wsId, CancellationToken cancellationToken);

    private async Task HandleEventMessage(
        System.Net.WebSockets.WebSocket socket,
        string wsId,
        MessageEvent messageEvent,
        JsonDocument jsonDocument,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        switch (messageEvent)
        {
            case MessageEvent.Connect:
            {
                cancellationTokenWrapper.EnsureNonCancelledBroadcastCancellationTokenSource();
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<ConnectEvent>(MessageEvent.Connect)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.ConnectEvent)!;
                await HandleConnectOrExitStandby(socket, wsId, cancellationTokenWrapper);
                await OnConnect(payload, wsId, cancellationTokenWrapper.RequestAborted);
                return;
            }
            case MessageEvent.Disconnect:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<DisconnectEvent>(MessageEvent.Disconnect)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.DisconnectEvent)!;
                await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                var success = await OnDisconnect(payload, wsId, cancellationTokenWrapper.RequestAborted);
                RemoveSocketFromMap(wsId, out _);

                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateConnectEventResponsePayload(success ? DeviceState.Disconnected : DeviceState.Error),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);

                return;
            }
            case MessageEvent.AbortDriverSetup:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<AbortDriverSetupEvent>(MessageEvent.AbortDriverSetup)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.AbortDriverSetupEvent)!;
                await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                await OnAbortDriverSetup(payload, wsId, cancellationTokenWrapper.RequestAborted);
                if (RemoveSocketFromMap(wsId, out var entityId))
                {
                    await RemoveConfiguration(new RemoveInstruction(null, null, entityId), cancellationTokenWrapper.ApplicationStopping);
                    _logger.LogInformation("[{WSId}] WS: Removed configuration for {EntityId}", wsId, entityId);
                }
                
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(0),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.EnterStandby:
                {
                    var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<EnterStandbyEvent>(MessageEvent.EnterStandby)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.EnterStandbyEvent)!;
                    await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                    await OnEnterStandby(payload, wsId, cancellationTokenWrapper.RequestAborted);
                    await SendAsync(socket,
                        ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Disconnected),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }
            case MessageEvent.ExitStandby:
                {
                    var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<ExitStandbyEvent>(MessageEvent.ExitStandby)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.ExitStandbyEvent)!;
                    await OnExitStandby(payload, wsId, cancellationTokenWrapper.RequestAborted);
                    await HandleConnectOrExitStandby(socket, wsId, cancellationTokenWrapper);

                    return;
                }
            default:
                return;
        }
    }

    private async ValueTask HandleConnectOrExitStandby(System.Net.WebSockets.WebSocket socket, string wsId, CancellationTokenWrapper cancellationTokenWrapper)
    {
        cancellationTokenWrapper.EnsureNonCancelledBroadcastCancellationTokenSource();
        var configuration = await _configurationService.GetConfigurationAsync(cancellationTokenWrapper.RequestAborted);
        if (configuration is { Entities.Count: > 0 })
        {
            foreach (var unfoldedCircleConfigurationItem in configuration.Entities)
            {
                var entityState = await GetEntityState(unfoldedCircleConfigurationItem, wsId, cancellationTokenWrapper.RequestAborted);
                if (entityState is DeviceState.Connected)
                    _ = HandleEventUpdates(socket, unfoldedCircleConfigurationItem.EntityId, wsId, cancellationTokenWrapper);
            }
        }

        await SendAsync(socket,
            ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Connected),
            wsId,
            cancellationTokenWrapper.RequestAborted);
    }
}