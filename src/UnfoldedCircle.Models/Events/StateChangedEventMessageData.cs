using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Events;

public sealed record StateChangedEventMessageData<TAttributes>
    where TAttributes : StateChangedEventMessageDataAttributes
{
    [JsonPropertyName("entity_type")]
    public required EntityType EntityType { get; init; }

    [JsonPropertyName("entity_id")]
    public required string EntityId { get; init; }

    [JsonPropertyName("attributes")]
    public required TAttributes Attributes { get; init; }
}
