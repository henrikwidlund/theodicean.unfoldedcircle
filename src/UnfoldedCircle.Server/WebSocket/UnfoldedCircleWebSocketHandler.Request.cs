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
    /// <param name="commandCancellationToken">The <see cref="CancellationToken"/> for when commands should be aborted.</param>
    /// <remarks>
    /// Event applies to the <see cref="SubscribeEventsMsgData.DeviceId"/> and <see cref="SubscribeEventsMsgData.EntityIds"/> on the MessageData property,
    /// or all entities if both are null.
    /// </remarks>
    protected abstract ValueTask OnSubscribeEventsAsync(System.Net.WebSockets.WebSocket socket, SubscribeEventsMsg payload, string wsId, CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken);

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
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken cancellationToken)
    {
        switch (messageEvent)
        {
            case MessageEvent.GetDriverVersion:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<CommonReq>(MessageEvent.GetDriverVersion)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.CommonReq)!;
                var driverMetadata = await _configurationService.GetDriverMetadataAsync(cancellationToken);
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
                    cancellationToken);
                
                return;
            }
            case MessageEvent.GetDriverMetaData:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<CommonReq>(MessageEvent.GetDriverMetaData)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.CommonReq)!;
                
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDriverMetaDataResponsePayload(payload, await _configurationService.GetDriverMetadataAsync(cancellationToken)),
                    wsId,
                    cancellationToken);
                
                return;
            }
            case MessageEvent.GetDeviceState:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetDeviceStateMsg>(MessageEvent.GetDeviceState)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.GetDeviceStateMsg)!;
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceStateResponsePayload(
                        await OnGetDeviceStateAsync(payload, wsId, cancellationToken),
                        payload.MsgData.DeviceId
                    ),
                    wsId,
                    cancellationToken);

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
                            AvailableEntities = await OnGetAvailableEntitiesAsync(payload, wsId, cancellationToken)
                        }),
                    wsId,
                    cancellationToken);

                return;
            }
            case MessageEvent.SubscribeEvents:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<SubscribeEventsMsg>(MessageEvent.SubscribeEvents)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.SubscribeEventsMsg)!;
                AddSocketToEventReceivers(wsId);
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                    wsId,
                    cancellationToken);
                try
                {
                    await OnSubscribeEventsAsync(socket, payload, wsId, cancellationTokenWrapper, cancellationToken);
                }
                finally
                {
                    await cancellationTokenWrapper.StartEventProcessingAsync();
                }

                return;
            }
            case MessageEvent.UnsubscribeEvents:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<UnsubscribeEventsMsg>(MessageEvent.UnsubscribeEvents)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.UnsubscribeEventsMsg)!;
                await OnUnsubscribeEventsAsync(payload, wsId, cancellationTokenWrapper);
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                    wsId,
                    cancellationToken);
                await RemoveConfigurationAsync(wsId,
                    new RemoveInstruction(
                        payload.MsgData?.DeviceId.GetNullableBaseIdentifier(),
                        payload.MsgData?.EntityIds?.Select(static x => x.GetBaseIdentifier()), Host: null),
                    cancellationTokenWrapper.ApplicationStopping);

                return;
            }
            case MessageEvent.GetEntityStates:
            {
                GetEntityStatesMsg payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<GetEntityStatesMsg>(MessageEvent.GetEntityStates)
                                                                      ?? UnfoldedCircleJsonSerializerContext.Default.GetEntityStatesMsg)!;
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateGetEntityStatesResponsePayload(payload,
                        await OnGetEntityStatesAsync(payload, wsId, cancellationToken)),
                    wsId,
                    cancellationToken);
                
                return;
            }
            case MessageEvent.SetupDriver:
            {
                var payload = jsonDocument.Deserialize(GetCustomJsonTypeInfo<SetupDriverMsg>(MessageEvent.SetupDriver)
                                                       ?? UnfoldedCircleJsonSerializerContext.Default.SetupDriverMsg)!;
                var setupResult = await OnSetupDriverAsync(payload, wsId, cancellationTokenWrapper.ApplicationStopping);
                if (setupResult is null)
                {
                    _logger.DriverSetupFailed(wsId, payload.MsgData);

                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                            new ValidationError
                            {
                                Code = "ENTITY_NOT_FOUND",
                                Message = "Entity not found."
                            }),
                        wsId,
                        cancellationToken);
                    return;
                }

                if (setupResult.SetupDriverResult == SetupDriverResult.UserInputRequired)
                {
                    if (setupResult.NextSetupStep is null)
                        _logger.UserInputNoNextStep(wsId, payload.MsgData);
                    else
                    {
                        await Task.WhenAll(SendMessageAsync(socket,
                                ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                                wsId,
                                cancellationToken),
                            SendMessageAsync(socket,
                                ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(setupResult.NextSetupStep),
                                wsId,
                                cancellationToken));
                        return;
                    }
                }

                await Task.WhenAll(
                    SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                        wsId,
                        cancellationToken),
                    SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(setupResult.SetupDriverResult == SetupDriverResult.Finalized),
                        wsId,
                        cancellationToken),
                    setupResult.SetupDriverResult == SetupDriverResult.Error
                        ? Task.CompletedTask
                        : SendMessageAsync(socket,
                            ResponsePayloadHelpers.CreateConnectEventResponsePayload(DeviceState.Connected),
                            wsId,
                            cancellationToken)
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
                        await HandleEntityCommandAsync(socket, payload, wsId, cancellationTokenWrapper, cancellationToken);
                }
                else if (entityType == EntityType.Remote)
                {
                    var payload = GetRemoteCommandPayload(jsonDocument);
                    await HandleEntityCommandAsync(socket, payload, wsId, cancellationTokenWrapper, cancellationToken);
                }
                else
                    _logger.UnsupportedEntityType(wsId, entityType);

                return;
            }
            default:
                return;
        }
    }

    private static EntityType? GetEntityType(JsonDocument jsonDocument) =>
        jsonDocument.RootElement.TryGetProperty("msg_data", out var msgDataElement) && msgDataElement.TryGetProperty("entity_type", out var value)
            ? value.Deserialize(UnfoldedCircleJsonSerializerContext.Default.EntityType)
            : null;
}