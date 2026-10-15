namespace UnfoldedCircle.Models.Shared;

[JsonConverter(typeof(StrictEnumConverter<Kind>))]
public enum Kind : byte
{
    [JsonStringEnumMemberName("req")]
    Request = 1,

    [JsonStringEnumMemberName("resp")]
    Response,

    [JsonStringEnumMemberName("event")]
    Event
}
