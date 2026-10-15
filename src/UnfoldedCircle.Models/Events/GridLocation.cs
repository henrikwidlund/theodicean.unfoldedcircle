namespace UnfoldedCircle.Models.Events;

/// <summary>
/// Button placement in the grid with 0-based coordinates.
/// </summary>
public sealed record GridLocation
{
    [JsonPropertyName("x")]
    public ushort X { get; init; }

    [JsonPropertyName("y")]
    public ushort Y { get; init; }
}
