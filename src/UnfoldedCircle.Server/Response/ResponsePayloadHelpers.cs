using System.Text.Json.Serialization.Metadata;

using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Extensions;
using UnfoldedCircle.Server.Json;

namespace UnfoldedCircle.Server.Response;

/// <summary>
/// Helpers for creating response payloads for various requests and events in the Unfolded Circle server.
/// </summary>
public static class ResponsePayloadHelpers
{
    private const string EventKind = "event";
    private const string Device = "DEVICE";
    private const string DriverSetupChange = "driver_setup_change";

    private static byte[]? _createAuthResponsePayload;

    /// <summary>
    /// Creates a response payload for authentication requests.
    /// </summary>
    public static byte[] CreateAuthResponsePayload() =>
        _createAuthResponsePayload ??= JsonSerializer.SerializeToUtf8Bytes(new AuthMsg
            {
                Kind = "resp",
                ReqId = 0,
                Msg = "authentication",
                Code = 200
            },
            UnfoldedCircleJsonSerializerContext.Default.AuthMsg);

    /// <summary>
    /// Creates a response payload for driver version requests.
    /// </summary>
    /// <param name="req">The <see cref="CommonReq"/>.</param>
    /// <param name="driverVersion">The <see cref="DriverVersion"/>.</param>
    public static byte[] CreateDriverVersionResponsePayload(
        CommonReq req,
        DriverVersion driverVersion) =>
        JsonSerializer.SerializeToUtf8Bytes(new DriverVersionMsg
            {
                Kind = "resp",
                ReqId = req.Id,
                Msg = "driver_version",
                Code = 200,
                MsgData = driverVersion
            },
            UnfoldedCircleJsonSerializerContext.Default.DriverVersionMsg);

    /// <summary>
    /// Creates a response payload for driver metadata requests.
    /// </summary>
    /// <param name="req">The <see cref="CommonReq"/>.</param>
    /// <param name="driverMetadata">The <see cref="DriverMetadata"/>.</param>
    public static byte[] CreateDriverMetaDataResponsePayload(
        CommonReq req,
        DriverMetadata driverMetadata) =>
        JsonSerializer.SerializeToUtf8Bytes(new DriverMetadataMsg
            {
                Kind = "resp",
                ReqId = req.Id,
                Msg = "driver_metadata",
                Code = 200,
                MsgData = driverMetadata
            },
            UnfoldedCircleJsonSerializerContext.Default.DriverMetadataMsg);

    /// <summary>
    /// Creates an event payload for device state updates.
    /// </summary>
    /// <param name="deviceState">The device state.</param>
    /// <param name="deviceId">The device_id the state is for (optional).</param>
    public static byte[] CreateDeviceStateResponsePayload(
        in DeviceState deviceState,
        string? deviceId) =>
        JsonSerializer.SerializeToUtf8Bytes(new DeviceStateEventMsg
        {
            Kind = EventKind,
            Msg = "device_state",
            Cat = Device,
            TimeStamp = DateTime.UtcNow,
            MsgData = new DeviceStateItem
            {
                State = deviceState,
                DeviceId = deviceId
            }
        }, UnfoldedCircleJsonSerializerContext.Default.DeviceStateEventMsg);

    /// <summary>
    /// Creates a response payload for available entities requests.
    /// </summary>
    /// <param name="req">The <see cref="GetAvailableEntitiesMsg"/>.</param>
    /// <param name="availableEntitiesMsgData">The <see cref="AvailableEntitiesMsgData"/>.</param>
    public static byte[] CreateGetAvailableEntitiesMsg(
        GetAvailableEntitiesMsg req,
        AvailableEntitiesMsgData availableEntitiesMsgData) =>
        JsonSerializer.SerializeToUtf8Bytes(new AvailableEntitiesMsg
            {
                Kind = "resp",
                ReqId = req.Id,
                Msg = "available_entities",
                Code = 200,
                MsgData = availableEntitiesMsgData
            },
            UnfoldedCircleJsonSerializerContext.Default.AvailableEntitiesMsg);

    /// <summary>
    /// Creates a common response payload for requests that do not require additional data.
    /// </summary>
    /// <param name="req">The <see cref="CommonReq"/>.</param>
    public static byte[] CreateCommonResponsePayload(
        CommonReq req) =>
        JsonSerializer.SerializeToUtf8Bytes(new CommonResp
            {
                Code = 200,
                Kind = "resp",
                ReqId = req.Id,
                Msg = "result"
            },
            UnfoldedCircleJsonSerializerContext.Default.CommonResp);

    /// <summary>
    /// Creates a entity states response payload for a given request and entity states.
    /// </summary>
    /// <param name="req"></param>
    /// <param name="entityStates"></param>
    public static byte[] CreateGetEntityStatesResponsePayload(
        CommonReq req,
        EntityStateChanged[] entityStates) =>
        JsonSerializer.SerializeToUtf8Bytes(new EntityStates
        {
            Code = 200,
            Kind = "resp",
            ReqId = req.Id,
            Msg = "entity_states",
            MsgData = entityStates
        }, UnfoldedCircleJsonSerializerContext.Default.EntityStates);

    /// <summary>
    /// Creates an event payload used when setting up the driver.
    /// </summary>
    /// <param name="userAction">Instruction for the next setup step.</param>
    public static byte[] CreateDeviceSetupChangeResponsePayload(
        RequireUserAction userAction) =>
        JsonSerializer.SerializeToUtf8Bytes(new DriverSetupChangeEvent
        {
            Kind = EventKind,
            Msg = DriverSetupChange,
            Cat = Device,
            TimeStamp = DateTime.UtcNow,
            MsgData = new DriverSetupChange
            {
                State = DriverSetupChangeState.WaitUserAction,
                EventType = DriverSetupChangeEventType.Setup,
                RequireUserAction = userAction
            }
        }, UnfoldedCircleJsonSerializerContext.Default.DriverSetupChangeEvent);

    /// <summary>
    /// Creates an event payload used when setting up the driver.
    /// </summary>
    /// <param name="isSuccess">Whether the setup process succeeded or not.</param>
    public static byte[] CreateDeviceSetupChangeResponsePayload(
        in bool isSuccess) =>
        JsonSerializer.SerializeToUtf8Bytes(new DriverSetupChangeEvent
        {
            Kind = EventKind,
            Msg = DriverSetupChange,
            Cat = Device,
            TimeStamp = DateTime.UtcNow,
            MsgData = new DriverSetupChange
            {
                State = isSuccess ? DriverSetupChangeState.Ok : DriverSetupChangeState.Error,
                EventType = DriverSetupChangeEventType.Stop,
                Error = isSuccess ? null : DriverSetupChangeError.NotFound
            }
        }, UnfoldedCircleJsonSerializerContext.Default.DriverSetupChangeEvent);

    /// <summary>
    /// Creates an event payload used when setting up the driver.
    /// </summary>
    public static byte[] CreateDeviceSetupChangeResponseSetupPayload() =>
        JsonSerializer.SerializeToUtf8Bytes(new DriverSetupChangeEvent
        {
            Kind = EventKind,
            Msg = DriverSetupChange,
            Cat = Device,
            TimeStamp = DateTime.UtcNow,
            MsgData = new DriverSetupChange
            {
                State = DriverSetupChangeState.Setup,
                EventType = DriverSetupChangeEventType.Setup
            }
        }, UnfoldedCircleJsonSerializerContext.Default.DriverSetupChangeEvent);

    /// <summary>
    /// Creates an event payload used when setting up the driver, requesting user input via a settings page.
    /// </summary>
    /// <param name="settingsPage">The <see cref="SettingsPage"/> that will be rendered on the remote.</param>
    public static byte[] CreateDeviceSetupChangeUserInputResponsePayload(SettingsPage settingsPage) =>
        JsonSerializer.SerializeToUtf8Bytes(new DriverSetupChangeEvent
        {
            Kind = "event",
            Msg = DriverSetupChange,
            Cat = Device,
            TimeStamp = DateTime.UtcNow,
            MsgData = new DriverSetupChange
            {
                State = DriverSetupChangeState.WaitUserAction,
                EventType = DriverSetupChangeEventType.Setup,
                RequireUserAction = new RequireUserAction
                {
                    Input = settingsPage
                }
            }
        }, UnfoldedCircleJsonSerializerContext.Default.DriverSetupChangeEvent);

    /// <summary>
    /// Creates an event payload for a connect event, which includes the current device state.
    /// </summary>
    /// <param name="deviceState">The device state.</param>
    public static byte[] CreateConnectEventResponsePayload(
        in DeviceState deviceState) =>
        JsonSerializer.SerializeToUtf8Bytes(new ConnectEventMsg
        {
            Kind = EventKind,
            Msg = "device_state",
            Cat = Device,
            TimeStamp = DateTime.UtcNow,
            MsgData = new ConnectDeviceStateItem { State = deviceState }
        }, UnfoldedCircleJsonSerializerContext.Default.ConnectEventMsg);

    /// <summary>
    /// Creates a response payload signifying a validation error for a request.
    /// </summary>
    /// <param name="req">The <see cref="CommonReq"/>.</param>
    /// <param name="validationError">The <see cref="ValidationError"/>.</param>
    public static byte[] CreateValidationErrorResponsePayload(
        CommonReq req,
        ValidationError validationError) =>
        JsonSerializer.SerializeToUtf8Bytes(new CommonRespRequired<ValidationError>
        {
            Kind = "resp",
            ReqId = req.Id,
            Msg = "result",
            Code = 400,
            MsgData = validationError
        }, UnfoldedCircleJsonSerializerContext.Default.CommonRespRequiredValidationError);

    /// <summary>
    /// Creates an event payload for when a media player entity's state has changed.
    /// </summary>
    /// <param name="attributes">The attributes for the entity.</param>
    /// <param name="entityId">The entity_id.</param>
    public static byte[] CreateMediaPlayerStateChangedResponsePayload(
        MediaPlayerStateChangedEventMessageDataAttributes attributes,
        string entityId) =>
        CreateEntityStateChangedResponsePayload(attributes, entityId, EntityType.MediaPlayer,
            UnfoldedCircleJsonSerializerContext.Default.StateChangedEventMediaPlayerStateChangedEventMessageDataAttributes,
            null);

    /// <summary>
    /// Creates an event payload for when a remote entity's state has changed.
    /// </summary>
    /// <param name="attributes">The attributes for the entity.</param>
    /// <param name="entityId">The entity_id.</param>
    public static byte[] CreateRemoteStateChangedResponsePayload(
        RemoteStateChangedEventMessageDataAttributes attributes,
        string entityId) =>
        CreateEntityStateChangedResponsePayload(attributes, entityId, EntityType.Remote,
            UnfoldedCircleJsonSerializerContext.Default.StateChangedEventRemoteStateChangedEventMessageDataAttributes,
            null);

    /// <summary>
    /// Creates an event payload for when a sensor entity's state has changed.
    /// </summary>
    /// <param name="attributes">The attributes for the entity.</param>
    /// <param name="entityId">The entity_id.</param>
    /// <param name="suffix">
    /// Optional suffix to add to the identifier.
    /// </param>
    public static byte[] CreateSensorStateChangedResponsePayload<TValue>(
        SensorStateChangedEventMessageDataAttributes<TValue> attributes,
        string entityId,
        string? suffix)
    {
        return CreateEntityStateChangedResponsePayload(attributes, entityId, EntityType.Sensor,
            typeof(TValue) switch
            {
                var t when t == typeof(int) => UnfoldedCircleJsonSerializerContext.Default.StateChangedEventSensorStateChangedEventMessageDataAttributesInt32,
                var t when t == typeof(string) => UnfoldedCircleJsonSerializerContext.Default.StateChangedEventSensorStateChangedEventMessageDataAttributesString,
                var t when t == typeof(decimal) => UnfoldedCircleJsonSerializerContext.Default.StateChangedEventSensorStateChangedEventMessageDataAttributesDecimal,
                var t when t == typeof(double) => UnfoldedCircleJsonSerializerContext.Default.StateChangedEventSensorStateChangedEventMessageDataAttributesDouble,
                _ => throw new NotSupportedException($"The type '{typeof(TValue)}' is not supported for sensor state changed event message data attributes.")
            }, suffix);
    }

    /// <summary>
    /// Creates an event payload for when a climate entity's state has changed.
    /// </summary>
    /// <param name="attributes">The attributes for the entity.</param>
    /// <param name="entityId">The entity_id.</param>
    public static byte[] CreateClimateStateChangedResponsePayload(
        ClimateStateChangedEventMessageDataAttributes attributes,
        string entityId) =>
        CreateEntityStateChangedResponsePayload(attributes, entityId, EntityType.Climate,
            UnfoldedCircleJsonSerializerContext.Default.StateChangedEventClimateStateChangedEventMessageDataAttributes,
            null);

    /// <summary>
    /// Creates an event payload for when a select entity's state has changed.
    /// </summary>
    /// <param name="attributes">The attributes for the entity.</param>
    /// <param name="entityId">The entity_id.</param>
    /// <param name="suffix">
    /// Optional suffix to add to the identifier.
    /// </param>
    public static byte[] CreateSelectStateChangedResponsePayload(
        SelectStateChangedEventMessageDataAttributes attributes,
        string entityId,
        string? suffix) =>
        CreateEntityStateChangedResponsePayload(attributes, entityId, EntityType.Select,
            UnfoldedCircleJsonSerializerContext.Default.StateChangedEventSelectStateChangedEventMessageDataAttributes,
            suffix);

    private static byte[] CreateEntityStateChangedResponsePayload<TAttributes>(
        TAttributes attributes,
        string entityId,
        in EntityType entityType,
        JsonTypeInfo jsonTypeInfo,
        string? suffix) where TAttributes : StateChangedEventMessageDataAttributes =>
        JsonSerializer.SerializeToUtf8Bytes(new StateChangedEvent<TAttributes>
            {
                Kind = EventKind,
                Msg = "entity_change",
                Cat = "ENTITY",
                TimeStamp = DateTime.UtcNow,
                MsgData = new StateChangedEventMessageData<TAttributes>
                {
                    EntityId = entityId.GetIdentifier(entityType, suffix),
                    EntityType = entityType,
                    Attributes = attributes
                }
            },
            jsonTypeInfo);
}