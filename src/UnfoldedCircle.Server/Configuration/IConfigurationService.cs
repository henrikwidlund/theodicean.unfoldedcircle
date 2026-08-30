using UnfoldedCircle.Models.Sync;

namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Interface for working with configuration in the Unfolded Circle server.
/// </summary>
/// <typeparam name="TGlobalConfiguration">The type used for storing global configuration values.</typeparam>
/// <typeparam name="TConfigurationItem">The type used for storing entity specific configuration values.</typeparam>
public interface IConfigurationService<TGlobalConfiguration, TConfigurationItem>
    where TGlobalConfiguration : UnfoldedCircleGlobalConfiguration, new()
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    /// <summary>
    /// Gets the current configuration of the Unfolded Circle server.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task<UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>> GetConfigurationAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates the configuration of the Unfolded Circle server with the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to add or update</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task<UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>> UpdateConfigurationAsync(UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem> configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Gets metadata about the driver that is used in the setup flow and mDNS.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    ValueTask<DriverMetadata> GetDriverMetadataAsync(CancellationToken cancellationToken);
}
