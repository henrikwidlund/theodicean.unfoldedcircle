using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Events;
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
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnUnsubscribeEventsAsync(UnsubscribeEventsMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>get_entity_states</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A collection of <see cref="AvailableEntity"/>.</returns>
    protected abstract ValueTask<EntityStateChanged[]> OnGetEntityStatesAsync(GetEntityStatesMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <remarks>Any additional steps needed must be handled manually here by sending the instructions through <see cref="SendMessageAsync"/>.</remarks>
    protected abstract ValueTask OnSetupDriverUserDataAsync(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId, CancellationToken cancellationToken);

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

    /// <summary>
    /// Called when a <c>setup_driver</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Returns the found entity with its connection state, or null if not found.</returns>
    protected abstract ValueTask<OnSetupResult?> OnSetupDriverAsync(SetupDriverMsg payload, string wsId, CancellationToken cancellationToken);

    /// <summary>
    /// Record representing the result of a lookup of configuration item during setup.
    /// </summary>
    /// <param name="Entity">The configuration item.</param>
    /// <param name="SetupDriverResult">Result of the current setup step.</param>
    /// <param name="NextSetupStep">Information about the next setup step. Must be sent if <paramref name="SetupDriverResult"/> is set to <see cref="SetupDriverResult.UserInputRequired"/>.</param>
    protected sealed record OnSetupResult(TConfigurationItem Entity, in SetupDriverResult SetupDriverResult, RequireUserAction? NextSetupStep = null);

    /// <summary>
    /// Setup driver result.
    /// </summary>
    protected enum SetupDriverResult
    {
        /// <summary>
        /// Setup finished successfully.
        /// </summary>
        Finalized,

        /// <summary>
        /// User input is required to continue the setup process.
        /// </summary>
        UserInputRequired,

        /// <summary>
        /// Error occurred during setup.
        /// </summary>
        Error
    }

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
                await OnSubscribeEventsAsync(socket, payload, wsId, cancellationTokenWrapper);
                await SendMessageAsync(socket,
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
                await OnUnsubscribeEventsAsync(payload, wsId, cancellationTokenWrapper.RequestAborted);
                await RemoveConfigurationAsync(new RemoveInstruction(payload.MsgData?.DeviceId.GetNullableBaseIdentifier(), payload.MsgData?.EntityIds?.Select(static x => x.GetBaseIdentifier()), null), cancellationTokenWrapper.ApplicationStopping);
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
                                Code = "INV_ARGUMENT",
                                Message = "Entity not found."
                            }),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }
                MapEntityIdToSocket(wsId, setupResult.Entity.EntityId);
                if (setupResult.SetupDriverResult == SetupDriverResult.UserInputRequired)
                {
                    if (setupResult.NextSetupStep is null)
                    {
                        _logger.LogError("[{WSId}] WS: Setup driver user input required but no next setup step provided. Setup will be aborted. Payload: {@Payload}.",
                            wsId, payload.MsgData);
                    }
                    else
                    {
                        await SendMessageAsync(socket,
                            ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(setupResult.NextSetupStep),
                            wsId,
                            cancellationTokenWrapper.RequestAborted);
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
                await OnSetupDriverUserDataAsync(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);

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