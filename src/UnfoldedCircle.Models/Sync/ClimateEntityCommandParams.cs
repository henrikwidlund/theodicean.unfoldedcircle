using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

public record ClimateEntityCommandParams
{
    [JsonPropertyName("hvac_mode")]
    public HvacMode? HvacMode { get; init; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }
}