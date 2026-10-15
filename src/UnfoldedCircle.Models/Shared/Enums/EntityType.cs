namespace UnfoldedCircle.Models.Shared;

[JsonConverter(typeof(StrictEnumConverter<EntityType>))]
public enum EntityType : byte
{
    [JsonStringEnumMemberName("button")]
    Button = 1,

    [JsonStringEnumMemberName("climate")]
    Climate,

    [JsonStringEnumMemberName("cover")]
    Cover,

    [JsonStringEnumMemberName("light")]
    Light,

    [JsonStringEnumMemberName("media_player")]
    MediaPlayer,

    [JsonStringEnumMemberName("remote")]
    Remote,

    [JsonStringEnumMemberName("select")]
    Select,

    [JsonStringEnumMemberName("sensor")]
    Sensor,

    [JsonStringEnumMemberName("switch")]
    Switch
}
