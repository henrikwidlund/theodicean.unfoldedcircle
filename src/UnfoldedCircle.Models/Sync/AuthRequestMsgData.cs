namespace UnfoldedCircle.Models.Sync;

public record AuthRequestMsgData
{
    [JsonPropertyName("token")]
    // ReSharper disable once UnusedMember.Global
    public required string Token { get; init; }
}