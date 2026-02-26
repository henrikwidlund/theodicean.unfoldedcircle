namespace UnfoldedCircle.Models.Events;

public record SelectStateChangedEventMessageDataAttributes : StateChangedEventMessageDataAttributes
{
    /// <summary>
    /// Optional state of the select entity.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SelectState? State { get; init; }

    /// <summary>
    /// The currently selected option.
    /// </summary>
    [JsonPropertyName("current_option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? CurrentOption { get; init; }

    /// <summary>
    /// The available options to choose from.
    /// </summary>
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Options { get; init; }
}