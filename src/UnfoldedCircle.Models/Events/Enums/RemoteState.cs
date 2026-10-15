namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<RemoteState>))]
public enum RemoteState : byte
{
    [JsonStringEnumMemberName("ON")]
    On,

    [JsonStringEnumMemberName("OFF")]
    Off,

    [JsonStringEnumMemberName("UNAVAILABLE")]
    Unavailable,

    [JsonStringEnumMemberName("UNKNOWN")]
    Unknown
}
