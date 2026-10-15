namespace UnfoldedCircle.Models.Sync;

public sealed record ValidationError
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
