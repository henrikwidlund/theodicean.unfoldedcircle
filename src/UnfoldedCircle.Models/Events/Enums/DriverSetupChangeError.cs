namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<DriverSetupChangeError>))]
public enum DriverSetupChangeError : byte
{
    [JsonStringEnumMemberName("NONE")]
    None = 1,

    [JsonStringEnumMemberName("NOT_FOUND")]
    NotFound,

    [JsonStringEnumMemberName("CONNECTION_REFUSED")]
    ConnectionRefused,

    [JsonStringEnumMemberName("AUTHORIZATION_ERROR")]
    AuthorizationError,

    [JsonStringEnumMemberName("TIMEOUT")]
    Timeout,

    [JsonStringEnumMemberName("OTHER")]
    Other
}
