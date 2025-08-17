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
    protected abstract ValueTask OnConnectAsync(ConnectEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>disconnect</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns><see langword="true"/> if disconnect was successful, otherwise <see langword="false"/>.</returns>
    protected abstract ValueTask<bool> OnDisconnectAsync(DisconnectEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>abort_driver_setup</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnAbortDriverSetupAsync(AbortDriverSetupEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>enter_standby</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnEnterStandbyAsync(EnterStandbyEvent payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>exit_standby</c> event is received.
    /// </summary>
    /// <param name="payload">Payload of the event.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnExitStandbyAsync(ExitStandbyEvent payload, string wsId, CancellationToken cancellationToken);

    private async Task HandleEventMessageAsync(
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
                await HandleConnectOrExitStandbyAsync(socket, wsId, cancellationTokenWrapper);
                await OnConnectAsync(payload, wsId, cancellationTokenWrapper.RequestAborted);
                return;
            }
            case MessageEvent.Disconnect:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<DisconnectEvent>(MessageEvent.Disconnect)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.DisconnectEvent)!;
                await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                var success = await OnDisconnectAsync(payload, wsId, cancellationTokenWrapper.RequestAborted);
                RemoveSocketFromMap(wsId, out _);

                await SendMessageAsync(socket,
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
                await OnAbortDriverSetupAsync(payload, wsId, cancellationTokenWrapper.RequestAborted);
                if (RemoveSocketFromMap(wsId, out var entityId))
                {
                    await RemoveConfigurationAsync(new RemoveInstruction(null, null, entityId), cancellationTokenWrapper.ApplicationStopping);
                    _logger.LogInformation("[{WSId}] WS: Removed configuration for {EntityId}", wsId, entityId);
                }
                
                await SendMessageAsync(socket,
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
                    await OnEnterStandbyAsync(payload, wsId, cancellationTokenWrapper.RequestAborted);
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Disconnected),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }
            case MessageEvent.ExitStandby:
                {
                    var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<ExitStandbyEvent>(MessageEvent.ExitStandby)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.ExitStandbyEvent)!;
                    await OnExitStandbyAsync(payload, wsId, cancellationTokenWrapper.RequestAborted);
                    await HandleConnectOrExitStandbyAsync(socket, wsId, cancellationTokenWrapper);

                    return;
                }
            default:
                return;
        }
    }

    private async ValueTask HandleConnectOrExitStandbyAsync(System.Net.WebSockets.WebSocket socket, string wsId, CancellationTokenWrapper cancellationTokenWrapper)
    {
        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Connected),
            wsId,
            cancellationTokenWrapper.RequestAborted);

        cancellationTokenWrapper.EnsureNonCancelledBroadcastCancellationTokenSource();
        var configuration = await _configurationService.GetConfigurationAsync(cancellationTokenWrapper.RequestAborted);
        if (configuration is { Entities.Count: > 0 })
        {
            var entityStateCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            foreach (var entities in configuration.Entities.Chunk(EnvironmentConstants.MaxConcurrency))
                await Task.WhenAll(entities.Select(x => StartEventUpdatesAsync(x, entityStateCancellationTokenSource.Token)));
        }

        return;

        async Task StartEventUpdatesAsync(TConfigurationItem unfoldedCircleConfigurationItem, CancellationToken cancellationToken)
        {
            EntityState entityState;
            try
            {
                entityState = await GetEntityStateAsync(unfoldedCircleConfigurationItem, wsId, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, "[{WSId}] WS: Failed to get entity state for {EntityId} due to cancellation.",
                    wsId, unfoldedCircleConfigurationItem.EntityId);
                return;
            }

            if (entityState is EntityState.Connected)
                _ = Task.Factory.StartNew(() => HandleEventUpdatesAsync(socket, unfoldedCircleConfigurationItem.EntityId, wsId, cancellationTokenWrapper),
                    TaskCreationOptions.LongRunning);
        }
    }
}