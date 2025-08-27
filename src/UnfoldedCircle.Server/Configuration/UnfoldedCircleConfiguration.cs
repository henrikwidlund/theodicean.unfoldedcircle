namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Base configuration for Unfolded Circle server. Used to store information about all entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public record UnfoldedCircleConfiguration<TEntity> where TEntity : UnfoldedCircleConfigurationItem
{
    /// <summary>
    /// List of entities in the Unfolded Circle configuration. Each entity represents an entity that can be managed or controlled by the integration.
    /// </summary>
    public required List<TEntity> Entities { get; init; }

    /// <summary>
    /// The maximum wait time for a received message to be handled before being cancelled.
    /// Must not exceed 10 seconds as that is the timeout Core uses for responses.
    /// </summary>
    public double? MaxMessageHandlingWaitTimeInSeconds { get; init; }
}