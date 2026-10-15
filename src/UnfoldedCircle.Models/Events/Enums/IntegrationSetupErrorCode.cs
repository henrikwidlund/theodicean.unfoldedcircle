namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<IntegrationSetupErrorCode>))]
public enum IntegrationSetupErrorCode : byte
{
    [JsonStringEnumMemberName("NONE")]
    None = 1,

    [JsonStringEnumMemberName("NOT_FOUND")]
    NotFound,

    [JsonStringEnumMemberName("CONFIGURATION_REFUSED")]
    ConfigurationRefused,

    [JsonStringEnumMemberName("AUTHORIZATION_ERROR")]
    AuthorizationError,

    [JsonStringEnumMemberName("TIMEOUT")]
    Timeout,

    [JsonStringEnumMemberName("OTHER")]
    Other
}
