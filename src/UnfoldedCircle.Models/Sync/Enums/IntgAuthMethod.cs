namespace UnfoldedCircle.Models.Sync;

/// <summary>
/// Integration driver authentication method if a token is required.
///
/// The JSON `auth` message is used if a token is configured but no authentication method is set.
/// </summary>
[JsonConverter(typeof(StrictEnumConverter<IntgAuthMethod>))]
public enum IntgAuthMethod : byte
{
    [JsonStringEnumMemberName("HEADER")]
    Header = 1,

    [JsonStringEnumMemberName("MESSAGE")]
    Message
}
