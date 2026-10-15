namespace UnfoldedCircle.Models.Sync;

public sealed record DriverVersionInner
{
    [JsonPropertyName("api")]
    public string? Api { get; init; }

    [JsonPropertyName("driver")]
    public string? Driver { get; init; }
}
