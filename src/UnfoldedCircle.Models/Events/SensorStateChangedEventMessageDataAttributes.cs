namespace UnfoldedCircle.Models.Events;

public record SensorStateChangedEventMessageDataAttributes<TValue> : StateChangedEventMessageDataAttributes
{
    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public SensorState? State { get; init; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required TValue? Value { get; init; }

    [JsonPropertyName("unit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Unit { get; init; }
}