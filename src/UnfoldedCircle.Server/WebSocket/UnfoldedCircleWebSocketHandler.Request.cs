using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Event;
using UnfoldedCircle.Server.Extensions;
using UnfoldedCircle.Server.Json;
using UnfoldedCircle.Server.Response;

namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    /// <summary>
    /// Called when a <c>get_device_state</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DeviceState"/>.</returns>
    protected abstract ValueTask<DeviceState> OnGetDeviceState(GetDeviceStateMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the device state for the given <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to get device state for.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<DeviceState> GetDeviceState(TConfigurationItem entity, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>get_available_entities</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A collection of <see cref="AvailableEntity"/>.</returns>
    protected abstract ValueTask<IReadOnlyCollection<AvailableEntity>> OnGetAvailableEntities(GetAvailableEntitiesMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>subscribe_events</c> request is received.
    /// </summary>
    /// <param name="socket">The socket for the current session.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/>.</param>
    protected abstract ValueTask OnSubscribeEvents(System.Net.WebSockets.WebSocket socket, CommonReq payload, string wsId, CancellationTokenWrapper cancellationTokenWrapper);

    /// <summary>
    /// Called when a <c>unsubscribe_events</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnUnsubscribeEvents(UnsubscribeEventsMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>get_entity_states</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A collection of <see cref="AvailableEntity"/>.</returns>
    protected abstract ValueTask<EntityStateChanged[]> OnGetEntityStates(GetEntityStatesMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <remarks>Any additional steps needed must be handled manually here by sending the instructions through <see cref="SendAsync"/>.</remarks>
    protected abstract ValueTask OnSetupDriverUserData(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the payload of a media player command message.
    /// </summary>
    /// <param name="jsonDocument">The <see cref="JsonDocument"/> that should be deserialized.</param>
    protected abstract MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> DeserializeMediaPlayerCommandPayload(JsonDocument jsonDocument);

    /// <summary>
    /// Deserializes the payload of a remote entity command message.
    /// </summary>
    /// <param name="jsonDocument">The <see cref="JsonDocument"/> that should be deserialized.</param>
    protected virtual RemoteEntityCommandMsgData GetRemoteCommandPayload(JsonDocument jsonDocument)
        => jsonDocument.Deserialize<RemoteEntityCommandMsgData>(UnfoldedCircleJsonSerializerContext.Default.RemoteEntityCommandMsgData)!;

    /// <summary>
    /// Called when a <c>setup_driver</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Returns the found entity with its connection state, or null if not found.</returns>
    protected abstract ValueTask<OnSetupResult?> OnSetupDriver(SetupDriverMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Record representing the result of a lookup of configuration item during setup.
    /// </summary>
    /// <param name="Entity">The configuration item.</param>
    /// <param name="IsConnected">Whether the entity is connected or not.</param>
    protected sealed record OnSetupResult(TConfigurationItem Entity, bool IsConnected);

    private async Task HandleRequestMessage(
        System.Net.WebSockets.WebSocket socket,
        string wsId,
        MessageEvent messageEvent,
        JsonDocument jsonDocument,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        switch (messageEvent)
        {
            case MessageEvent.GetDriverVersion:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<CommonReq>(MessageEvent.GetDriverVersion)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.CommonReq)!;
                var driverMetadata = await _configurationService.GetDriverMetadataAsync(cancellationTokenWrapper.RequestAborted);
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateDriverVersionResponsePayload(
                        payload,
                        new DriverVersion
                        {
                            Name = driverMetadata.Name["en"],
                            Version = new DriverVersionInner
                            {
                                Driver = driverMetadata.Version
                            }
                        }),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.GetDriverMetaData:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<CommonReq>(MessageEvent.GetDriverMetaData)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.CommonReq)!;
                
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateDriverMetaDataResponsePayload(payload, await _configurationService.GetDriverMetadataAsync(cancellationTokenWrapper.RequestAborted)),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.GetDeviceState:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetDeviceStateMsg>(MessageEvent.GetDeviceState)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.GetDeviceStateMsg)!;
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceStateResponsePayload(
                        await OnGetDeviceState(payload, wsId, cancellationTokenWrapper.RequestAborted),
                        payload.MsgData.DeviceId
                    ),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);

                return;
            }
            case MessageEvent.GetAvailableEntities:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetAvailableEntitiesMsg>(MessageEvent.GetAvailableEntities)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.GetAvailableEntitiesMsg)!;
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateGetAvailableEntitiesMsg(payload,
                        new AvailableEntitiesMsgData
                        {
                            Filter = payload.MsgData.Filter,
                            AvailableEntities = await OnGetAvailableEntities(payload, wsId, cancellationTokenWrapper.RequestAborted)
                        }),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);

                return;
            }
            case MessageEvent.SubscribeEvents:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<CommonReq>(MessageEvent.SubscribeEvents)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.CommonReq)!;
                await OnSubscribeEvents(socket, payload, wsId, cancellationTokenWrapper);
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);

                AddSocketToEventReceivers(wsId);

                return;
            }
            case MessageEvent.UnsubscribeEvents:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<UnsubscribeEventsMsg>(MessageEvent.UnsubscribeEvents)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.UnsubscribeEventsMsg)!;
                await OnUnsubscribeEvents(payload, wsId, cancellationTokenWrapper.RequestAborted);
                await RemoveConfiguration(new RemoveInstruction(payload.MsgData?.DeviceId.GetNullableBaseIdentifier(), payload.MsgData?.EntityIds?.Select(static x => x.GetBaseIdentifier()), null), cancellationTokenWrapper.ApplicationStopping);
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.GetEntityStates:
            {
                GetEntityStatesMsg payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetEntityStatesMsg>(MessageEvent.GetEntityStates)
                                                                      ?? UnfoldedCircleJsonSerializerContext.Default.GetEntityStatesMsg)!;
                await SendAsync(socket,
                    ResponsePayloadHelpers.CreateGetEntityStatesResponsePayload(payload,
                        await OnGetEntityStates(payload, wsId, cancellationTokenWrapper.RequestAborted)),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.SetupDriver:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<SetupDriverMsg>(MessageEvent.SetupDriver)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.SetupDriverMsg)!;
                var setupResult = await OnSetupDriver(payload, wsId, cancellationTokenWrapper.ApplicationStopping);
                if (setupResult is null)
                {
                    _logger.LogError("[{WSId}] WS: Setup driver failed. Payload: {@Payload}.", wsId, payload.MsgData);
                    await SendAsync(socket,
                        ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                            new ValidationError
                            {
                                Code = "INV_ARGUMENT",
                                Message = "Entity not found."
                            }),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }
                MapEntityIdToSocket(wsId, setupResult.Entity.EntityId);

                await Task.WhenAll(
                    SendAsync(socket,
                        ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                        wsId,
                        cancellationTokenWrapper.RequestAborted),
                    SendAsync(socket,
                        ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(setupResult.IsConnected),
                        wsId,
                        cancellationTokenWrapper.RequestAborted),
                    SendAsync(socket,
                        ResponsePayloadHelpers.CreateConnectEventResponsePayload(await GetDeviceState(setupResult.Entity, wsId, cancellationTokenWrapper.RequestAborted)),
                        wsId,
                        cancellationTokenWrapper.RequestAborted)
                );
                
                return;
            }
            case MessageEvent.SetupDriverUserData:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<SetDriverUserDataMsg>(MessageEvent.SetupDriverUserData)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.SetDriverUserDataMsg)!;
                await OnSetupDriverUserData(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);

                return;
            }
            case MessageEvent.EntityCommand:
            {
                var entityType = GetEntityType(jsonDocument);
                if (entityType == EntityType.MediaPlayer)
                {
                    var payload = DeserializeMediaPlayerCommandPayload(jsonDocument);
                    await HandleEntityCommand(socket, payload, wsId, cancellationTokenWrapper);
                }
                else if (entityType == EntityType.Remote)
                {
                    var payload = GetRemoteCommandPayload(jsonDocument);
                    await HandleEntityCommand(socket, payload, wsId, cancellationTokenWrapper);
                }
                else
                {
                    _logger.LogError("[{WSId}] WS: Unsupported entity type {EntityType}.",
                        wsId, entityType.ToString());
                }

                return;
            }
            default:
                return;
        }
    }

    private static EntityType? GetEntityType(JsonDocument jsonDocument)
    {
        return jsonDocument.RootElement.TryGetProperty("msg_data", out var msgDataElement) && msgDataElement.TryGetProperty("entity_type", out var value)
            ? value.Deserialize(UnfoldedCircleJsonSerializerContext.Default.EntityType)
            : null;
    }
}