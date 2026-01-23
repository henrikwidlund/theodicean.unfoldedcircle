namespace UnfoldedCircle.Models.Sync;

public record SubscribeEventsMsgData
{
    /// <summary>
    /// Only required for multi-device integrations.
    /// </summary>
    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    /// <summary>
    /// Subscribe to events only for specified entities.
    /// </summary>
    [JsonPropertyName("entity_ids")]
    public string[]? EntityIds { get; init; }
}