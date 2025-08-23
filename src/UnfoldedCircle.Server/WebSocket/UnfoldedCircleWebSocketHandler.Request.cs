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
    protected abstract ValueTask<DeviceState> OnGetDeviceStateAsync(GetDeviceStateMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entity state for the given <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to get the state for.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<EntityState> GetEntityStateAsync(TConfigurationItem entity, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Represents the current state of an entity.
    /// </summary>
    protected enum EntityState
    {
        /// <summary>
        /// Entity is connected and operational.
        /// </summary>
        Connected,

        /// <summary>
        /// Entity is in a disconnected state.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Error while processing the entity state.
        /// </summary>
        Error
    }

    /// <summary>
    /// Called when a <c>get_available_entities</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A collection of <see cref="AvailableEntity"/>.</returns>
    protected abstract ValueTask<IReadOnlyCollection<AvailableEntity>> OnGetAvailableEntitiesAsync(GetAvailableEntitiesMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>subscribe_events</c> request is received.
    /// </summary>
    /// <param name="socket">The socket for the current session.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/>.</param>
    protected abstract ValueTask OnSubscribeEventsAsync(System.Net.WebSockets.WebSocket socket, CommonReq payload, string wsId, CancellationTokenWrapper cancellationTokenWrapper);

    /// <summary>
    /// Called when a <c>unsubscribe_events</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/>.</param>
    /// <remarks>
    /// Event applies to the <see cref="UnsubscribeEventsMsgData.DeviceId"/> and <see cref="UnsubscribeEventsMsgData.EntityIds"/> on the MessageData property,
    /// or all entities if both are null.
    /// </remarks>
    protected abstract ValueTask OnUnsubscribeEventsAsync(UnsubscribeEventsMsg payload, string wsId, CancellationTokenWrapper cancellationTokenWrapper);

    /// <summary>
    /// Called when a <c>get_entity_states</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A collection of <see cref="AvailableEntity"/>.</returns>
    protected abstract ValueTask<EntityStateChanged[]> OnGetEntityStatesAsync(GetEntityStatesMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the payload of a media player command message.
    /// </summary>
    /// <param name="jsonDocument">The <see cref="JsonDocument"/> that should be deserialized.</param>
    protected abstract MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId>? DeserializeMediaPlayerCommandPayload(JsonDocument jsonDocument);

    /// <summary>
    /// Deserializes the payload of a remote entity command message.
    /// </summary>
    /// <param name="jsonDocument">The <see cref="JsonDocument"/> that should be deserialized.</param>
    protected virtual RemoteEntityCommandMsgData GetRemoteCommandPayload(JsonDocument jsonDocument)
        => jsonDocument.Deserialize<RemoteEntityCommandMsgData>(UnfoldedCircleJsonSerializerContext.Default.RemoteEntityCommandMsgData)!;

    private async Task HandleRequestMessageAsync(
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
                await SendMessageAsync(socket,
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
                
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDriverMetaDataResponsePayload(payload, await _configurationService.GetDriverMetadataAsync(cancellationTokenWrapper.RequestAborted)),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.GetDeviceState:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetDeviceStateMsg>(MessageEvent.GetDeviceState)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.GetDeviceStateMsg)!;
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceStateResponsePayload(
                        await OnGetDeviceStateAsync(payload, wsId, cancellationTokenWrapper.RequestAborted),
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
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateGetAvailableEntitiesMsg(payload,
                        new AvailableEntitiesMsgData
                        {
                            Filter = payload.MsgData.Filter,
                            AvailableEntities = await OnGetAvailableEntitiesAsync(payload, wsId, cancellationTokenWrapper.RequestAborted)
                        }),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);

                return;
            }
            case MessageEvent.SubscribeEvents:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<CommonReq>(MessageEvent.SubscribeEvents)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.CommonReq)!;
                AddSocketToEventReceivers(wsId);
                cancellationTokenWrapper.EnsureNonCancelledBroadcastCancellationTokenSource();
                await OnSubscribeEventsAsync(socket, payload, wsId, cancellationTokenWrapper);
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);

                return;
            }
            case MessageEvent.UnsubscribeEvents:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<UnsubscribeEventsMsg>(MessageEvent.UnsubscribeEvents)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.UnsubscribeEventsMsg)!;
                await OnUnsubscribeEventsAsync(payload, wsId, cancellationTokenWrapper);
                await RemoveConfigurationAsync(wsId,
                    new RemoveInstruction(
                        payload.MsgData?.DeviceId.GetNullableBaseIdentifier(),
                        payload.MsgData?.EntityIds?.Select(static x => x.GetBaseIdentifier()), null),
                    cancellationTokenWrapper.ApplicationStopping);
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.GetEntityStates:
            {
                GetEntityStatesMsg payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetEntityStatesMsg>(MessageEvent.GetEntityStates)
                                                                      ?? UnfoldedCircleJsonSerializerContext.Default.GetEntityStatesMsg)!;
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateGetEntityStatesResponsePayload(payload,
                        await OnGetEntityStatesAsync(payload, wsId, cancellationTokenWrapper.RequestAborted)),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                
                return;
            }
            case MessageEvent.SetupDriver:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<SetupDriverMsg>(MessageEvent.SetupDriver)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.SetupDriverMsg)!;
                var setupResult = await OnSetupDriverAsync(payload, wsId, cancellationTokenWrapper.ApplicationStopping);
                if (setupResult is null)
                {
                    _logger.LogError("[{WSId}] WS: Setup driver failed. Payload: {@Payload}.", wsId, payload.MsgData);
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                            new ValidationError
                            {
                                Code = "404",
                                Message = "Entity not found."
                            }),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }

                if (setupResult.SetupDriverResult == SetupDriverResult.UserInputRequired)
                {
                    if (setupResult.NextSetupStep is null)
                    {
                        _logger.LogError("[{WSId}] WS: Setup driver user input required but no next setup step provided. Setup will be aborted. Payload: {@Payload}.",
                            wsId, payload.MsgData);
                    }
                    else
                    {
                        await Task.WhenAll(SendMessageAsync(socket,
                                ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                                wsId,
                                cancellationTokenWrapper.RequestAborted),
                            SendMessageAsync(socket,
                                ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(setupResult.NextSetupStep),
                                wsId,
                                cancellationTokenWrapper.RequestAborted));
                        return;
                    }
                }

                await Task.WhenAll(
                    SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                        wsId,
                        cancellationTokenWrapper.RequestAborted),
                    SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(setupResult.SetupDriverResult == SetupDriverResult.Finalized),
                        wsId,
                        cancellationTokenWrapper.RequestAborted),
                    setupResult.SetupDriverResult == SetupDriverResult.Error
                        ? Task.CompletedTask
                        : SendMessageAsync(socket,
                            ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Connected),
                            wsId,
                            cancellationTokenWrapper.RequestAborted)
                );
                
                return;
            }
            case MessageEvent.SetupDriverUserData:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<SetDriverUserDataMsg>(MessageEvent.SetupDriverUserData)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.SetDriverUserDataMsg)!;

                await HandleSetupDriverUserData(socket, wsId, payload, cancellationTokenWrapper);
                return;
            }
            case MessageEvent.EntityCommand:
            {
                var entityType = GetEntityType(jsonDocument);
                if (entityType == EntityType.MediaPlayer)
                {
                    var payload = DeserializeMediaPlayerCommandPayload(jsonDocument);
                    if (payload is not null)
                        await HandleEntityCommandAsync(socket, payload, wsId, cancellationTokenWrapper);
                }
                else if (entityType == EntityType.Remote)
                {
                    var payload = GetRemoteCommandPayload(jsonDocument);
                    await HandleEntityCommandAsync(socket, payload, wsId, cancellationTokenWrapper);
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