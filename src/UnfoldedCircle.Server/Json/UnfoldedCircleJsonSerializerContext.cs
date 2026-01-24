using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Configuration;

namespace UnfoldedCircle.Server.Json;

/// <summary>
/// Json serializer context for Unfolded Circle server.
/// </summary>
[JsonSerializable(typeof(CommonReq))]
[JsonSerializable(typeof(CommonResp))]
[JsonSerializable(typeof(ConnectEvent))]
[JsonSerializable(typeof(DisconnectEvent))]
[JsonSerializable(typeof(AuthMsg))]
[JsonSerializable(typeof(DriverVersionMsg))]
[JsonSerializable(typeof(DriverMetadataMsg))]
[JsonSerializable(typeof(DeviceStateEventMsg))]
[JsonSerializable(typeof(ConnectEventMsg))]
[JsonSerializable(typeof(GetAvailableEntitiesMsg))]
[JsonSerializable(typeof(AvailableEntitiesMsg))]
[JsonSerializable(typeof(SetupDriverMsg))]
[JsonSerializable(typeof(DriverSetupChangeEvent))]
[JsonSerializable(typeof(MediaPlayerEntityCommandMsgData<MediaPlayerCommandId>))]
[JsonSerializable(typeof(RemoteEntityCommandMsgData))]
[JsonSerializable(typeof(CommonRespRequired<ValidationError>))]
[JsonSerializable(typeof(GetDeviceStateMsg))]
[JsonSerializable(typeof(SetDriverUserDataMsg))]
[JsonSerializable(typeof(AbortDriverSetupEvent))]
[JsonSerializable(typeof(GetEntityStatesMsg))]
[JsonSerializable(typeof(EntityStates))]
[JsonSerializable(typeof(UnfoldedCircleConfiguration<UnfoldedCircleConfigurationItem>))]
[JsonSerializable(typeof(SubscribeEventsMsg))]
[JsonSerializable(typeof(UnsubscribeEventsMsg))]
[JsonSerializable(typeof(EnterStandbyEvent))]
[JsonSerializable(typeof(ExitStandbyEvent))]
[JsonSerializable(typeof(StateChangedEvent<MediaPlayerStateChangedEventMessageDataAttributes>))]
[JsonSerializable(typeof(StateChangedEvent<RemoteStateChangedEventMessageDataAttributes>))]
[JsonSerializable(typeof(StateChangedEvent<SensorStateChangedEventMessageDataAttributes<int>>))]
[JsonSerializable(typeof(StateChangedEvent<SensorStateChangedEventMessageDataAttributes<string>>))]
[JsonSerializable(typeof(StateChangedEvent<SensorStateChangedEventMessageDataAttributes<decimal>>))]
[JsonSerializable(typeof(StateChangedEvent<SensorStateChangedEventMessageDataAttributes<double>>))]
[JsonSerializable(typeof(EntityType?))]
public sealed partial class UnfoldedCircleJsonSerializerContext : JsonSerializerContext
{
    static UnfoldedCircleJsonSerializerContext()
    {
        Default = new UnfoldedCircleJsonSerializerContext(new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new SettingTypeFieldConverter() }
        });
    }

    internal static readonly UnfoldedCircleJsonSerializerContext InstanceWithoutCustomConverters = new(new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });
}