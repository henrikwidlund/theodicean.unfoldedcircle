namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<SensorState>))]
public enum SensorState : byte
{
    [JsonStringEnumMemberName("ON")]
    On,

    [JsonStringEnumMemberName("UNAVAILABLE")]
    Unavailable,

    [JsonStringEnumMemberName("UNKNOWN")]
    Unknown
}
