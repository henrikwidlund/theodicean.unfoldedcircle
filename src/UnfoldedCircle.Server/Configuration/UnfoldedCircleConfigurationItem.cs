namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Base configuration item for Unfolded Circle server. Used to store information about entities.
/// </summary>
public record UnfoldedCircleConfigurationItem
{
    /// <summary>
    /// The host address of the entity. This is typically the IP address or hostname where the entity is reachable.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Optional device ID for the entity. This is a legacy property in the Unfolded Circle API.
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>
    /// Name of the entity.
    /// </summary>
    public required string EntityName { get; init; }

    /// <summary>
    /// ID of the entity. This is a unique identifier for the entity within the Unfolded Circle system.
    /// </summary>
    public required string EntityId { get; init; }
}