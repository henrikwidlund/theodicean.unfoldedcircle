namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Base configuration for Unfolded Circle server. Used to store information about all entities.
/// </summary>
/// <typeparam name="TGlobal"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public record UnfoldedCircleConfiguration<TGlobal, TEntity>
    where TGlobal : UnfoldedCircleGlobalConfiguration, new()
    where TEntity : UnfoldedCircleConfigurationItem
{
    /// <summary>
    /// List of entities in the Unfolded Circle configuration. Each entity represents an entity that can be managed or controlled by the integration.
    /// </summary>
    public required List<TEntity> Entities { get; init; }

    /// <summary>
    /// Global configuration for the Unfolded Circle server. This includes settings that apply to the entire server and all entities.
    /// </summary>
    public TGlobal GlobalConfiguration { get; init; } = new();

    /// <summary>
    /// The maximum wait time for a received message to be handled before being canceled.
    /// Must not exceed 10 seconds as that is the timeout Core uses for responses.
    /// </summary>
    [Obsolete("Use GlobalConfiguration.MaxMessageHandlingWaitTimeInSeconds instead.")]
    public double? MaxMessageHandlingWaitTimeInSeconds { get; init; }
}
