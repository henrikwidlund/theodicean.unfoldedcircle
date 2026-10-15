namespace UnfoldedCircle.Models.Shared;

[JsonConverter(typeof(StrictEnumConverter<DeviceState>))]
public enum DeviceState : byte
{
    [JsonStringEnumMemberName("CONNECTED")]
    Connected = 1,

    [JsonStringEnumMemberName("CONNECTING")]
    Connecting,

    [JsonStringEnumMemberName("DISCONNECTED")]
    Disconnected,

    [JsonStringEnumMemberName("ERROR")]
    Error
}
