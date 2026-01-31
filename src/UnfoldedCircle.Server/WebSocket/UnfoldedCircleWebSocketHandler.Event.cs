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
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<ConnectEvent>(MessageEvent.Connect)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.ConnectEvent)!;
                try
                {
                    await HandleConnectOrExitStandbyAsync(socket, wsId, cancellationToken);
                    await OnConnectAsync(payload, wsId, cancellationToken);
                }
                finally
                {
                    await cancellationTokenWrapper.StartEventProcessingAsync();
                }

                return;
            }
            case MessageEvent.Disconnect:
            {
                TryRemoveSocketBroadcastingEvents(wsId);
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<DisconnectEvent>(MessageEvent.Disconnect)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.DisconnectEvent)!;
                bool success;
                try
                {
                    success = await OnDisconnectAsync(payload, wsId, cancellationToken);
                }
                finally
                {
                    await cancellationTokenWrapper.StopEventProcessingAsync();
                }
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
                await OnAbortDriverSetupAsync(payload, wsId, cancellationToken);
                if (SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out var entityId))
                {
                    await RemoveConfigurationAsync(wsId, new RemoveInstruction(DeviceId: null, EntityIds: null, entityId), cancellationTokenWrapper.ApplicationStopping);
                    _logger.RemovedConfiguration(wsId, entityId);
                }
                
                return;
            }
            case MessageEvent.EnterStandby:
                {
                    var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<EnterStandbyEvent>(MessageEvent.EnterStandby)
                                                           ?? UnfoldedCircleJsonSerializerContext.Default.EnterStandbyEvent)!;
                    try
                    {
                        await OnEnterStandbyAsync(payload, wsId, cancellationToken);
                    }
                    finally
                    {
                        await cancellationTokenWrapper.StopEventProcessingAsync();
                    }

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
                    try
                    {
                        await OnExitStandbyAsync(payload, wsId, cancellationToken);
                        await HandleConnectOrExitStandbyAsync(socket, wsId, cancellationToken);
                    }
                    finally
                    {
                        await cancellationTokenWrapper.StartEventProcessingAsync();
                    }

                    return;
                }
            default:
                return;
        }
    }

    private async ValueTask HandleConnectOrExitStandbyAsync(System.Net.WebSockets.WebSocket socket,
        string wsId,
        CancellationToken cancellationToken) =>
        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Connected),
            wsId,
            cancellationToken);
}