namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<RemoteFeature>))]
public enum RemoteFeature : byte
{
    [JsonStringEnumMemberName("send_cmd")]
    SendCmd,

    [JsonStringEnumMemberName("on_off")]
    OnOff,

    [JsonStringEnumMemberName("toggle")]
    Toggle
}
