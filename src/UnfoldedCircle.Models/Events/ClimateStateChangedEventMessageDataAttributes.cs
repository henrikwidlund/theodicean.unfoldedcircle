namespace UnfoldedCircle.Models.Events;

public record ClimateStateChangedEventMessageDataAttributes : StateChangedEventMessageDataAttributes
{
    /// <summary>
    /// New HVAC mode.
    /// </summary>
    /// <see cref="ClimateState"/>
    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClimateState? State { get; init; }

    /// <summary>
    /// Current temperature value.
    /// </summary>
    [JsonPropertyName("current_temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? CurrentTemperature { get; init; }

    /// <summary>
    /// Changed target temperature value.
    /// </summary>
    [JsonPropertyName("target_temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? TargetTemperature { get; init; }
}