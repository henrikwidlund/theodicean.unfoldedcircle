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
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken cancellationToken)
    {
        switch (messageEvent)
        {
            case MessageEvent.Connect:
            {
                cancellationTokenWrapper.EnsureNonCancelledBroadcastCancellationTokenSource();
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<ConnectEvent>(MessageEvent.Connect)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.ConnectEvent)!;
                await HandleConnectOrExitStandbyAsync(socket, wsId, cancellationTokenWrapper, cancellationToken);
                await OnConnectAsync(payload, wsId, cancellationToken);
                return;
            }
            case MessageEvent.Disconnect:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<DisconnectEvent>(MessageEvent.Disconnect)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.DisconnectEvent)!;
                await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                var success = await OnDisconnectAsync(payload, wsId, cancellationToken);
                SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out _);

                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateConnectEventResponsePayload(success ? DeviceState.Disconnected : DeviceState.Error),
                    wsId,
                    cancellationToken);

                return;
            }
            case MessageEvent.AbortDriverSetup:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<AbortDriverSetupEvent>(MessageEvent.AbortDriverSetup)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.AbortDriverSetupEvent)!;
                await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                await OnAbortDriverSetupAsync(payload, wsId, cancellationToken);
                if (SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out var entityId))
                {
                    await RemoveConfigurationAsync(wsId, new RemoveInstruction(null, null, entityId), cancellationTokenWrapper.ApplicationStopping);
                    _logger.LogInformation("[{WSId}] WS: Removed configuration for {EntityId}", wsId, entityId);
                }
                
                return;
            }
            case MessageEvent.EnterStandby:
                {
                    var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<EnterStandbyEvent>(MessageEvent.EnterStandby)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.EnterStandbyEvent)!;
                    await (cancellationTokenWrapper.GetCurrentBroadcastCancellationTokenSource()?.CancelAsync() ?? Task.CompletedTask);
                    await OnEnterStandbyAsync(payload, wsId, cancellationToken);
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Disconnected),
                        wsId,
                        cancellationToken);
                    return;
                }
            case MessageEvent.ExitStandby:
                {
                    var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<ExitStandbyEvent>(MessageEvent.ExitStandby)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.ExitStandbyEvent)!;
                    await OnExitStandbyAsync(payload, wsId, cancellationToken);
                    await HandleConnectOrExitStandbyAsync(socket, wsId, cancellationTokenWrapper, cancellationToken);

                    return;
                }
            default:
                return;
        }
    }

    private async ValueTask HandleConnectOrExitStandbyAsync(System.Net.WebSockets.WebSocket socket,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken cancellationToken)
    {
        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Connected),
            wsId,
            cancellationToken);

        cancellationTokenWrapper.EnsureNonCancelledBroadcastCancellationTokenSource();
        var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
        if (configuration is { Entities.Count: > 0 })
        {
            await Parallel.ForEachAsync(configuration.Entities,
                new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = EnvironmentConstants.MaxConcurrency },
                async (item, token) =>
                {
                    await StartEventUpdatesAsync(item, token);
                });
        }

        return;

        async Task StartEventUpdatesAsync(TConfigurationItem unfoldedCircleConfigurationItem, CancellationToken eventCancellationToken)
        {
            EntityState entityState;
            try
            {
                entityState = await GetEntityStateAsync(unfoldedCircleConfigurationItem, wsId, eventCancellationToken);
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