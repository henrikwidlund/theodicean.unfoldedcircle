namespace UnfoldedCircle.Models.Sync;

public sealed record DriverVersion
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public DriverVersionInner? Version { get; init; }
}
