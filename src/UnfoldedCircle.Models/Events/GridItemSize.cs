namespace UnfoldedCircle.Models.Events;

/// <summary>
/// Item size in the button grid. Default size if not specified: 1 x 1
/// </summary>
public record GridItemSize(
    [property: JsonPropertyName("width")] ushort Width = 1,
    [property: JsonPropertyName("height")] ushort Height = 1);
