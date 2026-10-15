namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<State>))]
public enum State : byte
{
    [JsonStringEnumMemberName("UNAVAILABLE")]
    Unavailable,

    [JsonStringEnumMemberName("UNKNOWN")]
    Unknown,

    [JsonStringEnumMemberName("ON")]
    On,

    [JsonStringEnumMemberName("OFF")]
    Off,

    [JsonStringEnumMemberName("PLAYING")]
    Playing,

    [JsonStringEnumMemberName("PAUSED")]
    Paused,

    [JsonStringEnumMemberName("STANDBY")]
    Standby,

    [JsonStringEnumMemberName("BUFFERING")]
    Buffering
}
