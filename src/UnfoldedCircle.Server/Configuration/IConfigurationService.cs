using UnfoldedCircle.Models.Sync;

namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Interface for working with configuration in the Unfolded Circle server.
/// </summary>
/// <typeparam name="TConfigurationItem"></typeparam>
public interface IConfigurationService<TConfigurationItem> where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    /// <summary>
    /// Gets the current configuration of the Unfolded Circle server.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task<UnfoldedCircleConfiguration<TConfigurationItem>> GetConfigurationAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates the configuration of the Unfolded Circle server with the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to add or update</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task<UnfoldedCircleConfiguration<TConfigurationItem>> UpdateConfigurationAsync(UnfoldedCircleConfiguration<TConfigurationItem> configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Gets metadata about the driver that is used in the setup flow and mDNS.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    ValueTask<DriverMetadata> GetDriverMetadataAsync(CancellationToken cancellationToken);
}