namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<MediaType>))]
public enum MediaType : byte
{
    [JsonStringEnumMemberName("MUSIC")]
    Music,

    [JsonStringEnumMemberName("RADIO")]
    Radio,

    [JsonStringEnumMemberName("TVSHOW")]
    TvShow,

    [JsonStringEnumMemberName("MOVIE")]
    Movie,

    [JsonStringEnumMemberName("VIDEO")]
    Video
}
