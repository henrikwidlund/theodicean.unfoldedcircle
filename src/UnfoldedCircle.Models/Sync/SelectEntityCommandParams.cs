namespace UnfoldedCircle.Models.Sync;

public sealed record SelectEntityCommandParams
{
    [JsonPropertyName("option")]
    public string? Option { get; init; }

    [JsonPropertyName("cycle")]
    public bool? Cycle { get; init; }
}
