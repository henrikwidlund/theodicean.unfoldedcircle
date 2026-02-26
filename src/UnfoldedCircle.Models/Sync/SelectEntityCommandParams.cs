namespace UnfoldedCircle.Models.Sync;

public record SelectEntityCommandParams
{
    [JsonPropertyName("option")]
    public string? Option { get; init; }

    [JsonPropertyName("cycle")]
    public bool? Cycle { get; init; }
}