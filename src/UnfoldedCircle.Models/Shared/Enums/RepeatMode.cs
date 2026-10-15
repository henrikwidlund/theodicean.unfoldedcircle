namespace UnfoldedCircle.Models.Shared;

[JsonConverter(typeof(StrictEnumConverter<RepeatMode>))]
public enum RepeatMode : byte
{
    [JsonStringEnumMemberName("OFF")]
    Off = 1,

    [JsonStringEnumMemberName("ALL")]
    All,

    [JsonStringEnumMemberName("ONE")]
    One
}
