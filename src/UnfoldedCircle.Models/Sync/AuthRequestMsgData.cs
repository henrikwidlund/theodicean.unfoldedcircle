namespace UnfoldedCircle.Models.Sync;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record AuthRequestMsgData
{
    [JsonPropertyName("token")]
    // ReSharper disable once UnusedMember.Global
    public required string Token { get; init; }
}
