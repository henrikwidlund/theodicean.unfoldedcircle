using System.Text.Json.Serialization.Metadata;

using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Json;

namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Base class for configuration services that manage Unfolded Circle configurations.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/> used to determine the directory where files are stored.</param>
/// <typeparam name="TGlobalConfiguration">The type used for storing global values.</typeparam>
/// <typeparam name="TConfigurationItem">The type used for storing entity information.</typeparam>
/// <remarks>
/// Integration driver metadata is read from <c>driver.json</c> in the same directory as the program's executable.
/// Entity settings are stored in <c>configured_entities.json</c>, located in the <c>UC_CONFIG_HOME</c>,
/// or the same folder as the driver.json file if empty.
/// </remarks>
// ReSharper disable once UnusedType.Global
public abstract class ConfigurationService<TGlobalConfiguration, TConfigurationItem>(IConfiguration configuration) : IConfigurationService<TGlobalConfiguration, TConfigurationItem>
    where TGlobalConfiguration : UnfoldedCircleGlobalConfiguration, new()
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    private readonly IConfiguration _configuration = configuration;
    private string UcConfigHome => field ??= _configuration["UC_CONFIG_HOME"] ?? string.Empty;
    private string ConfigurationFilePath => Path.Combine(UcConfigHome, "configured_entities.json");
    private UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>? _unfoldedCircleConfiguration;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the configuration file can't be deserialized.</exception>
    public async Task<UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        if (_unfoldedCircleConfiguration is not null)
            return _unfoldedCircleConfiguration;

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_unfoldedCircleConfiguration is not null)
                return _unfoldedCircleConfiguration;

            if (File.Exists(ConfigurationFilePath))
            {
                await using var configurationFile = File.Open(ConfigurationFilePath, FileMode.Open);
                var deserialized = await JsonSerializer.DeserializeAsync(configurationFile,
                    GetSerializer(),
                    cancellationToken);

                _unfoldedCircleConfiguration = deserialized ?? throw new InvalidOperationException("Failed to deserialize configuration");
#pragma warning disable CS0618 // Needed for migration
                if (_unfoldedCircleConfiguration is { MaxMessageHandlingWaitTimeInSeconds: { } maxMessageHandlingWaitTimeInSeconds })
                {
                    _unfoldedCircleConfiguration = _unfoldedCircleConfiguration with
                    {
                        GlobalConfiguration = _unfoldedCircleConfiguration.GlobalConfiguration with
                        {
                            MaxMessageHandlingWaitTimeInSeconds = maxMessageHandlingWaitTimeInSeconds
                        },
                        MaxMessageHandlingWaitTimeInSeconds = null
                    };
                    await using var migratedConfigurationFile = File.Create(ConfigurationFilePath);
                    await JsonSerializer.SerializeAsync(migratedConfigurationFile, _unfoldedCircleConfiguration, GetSerializer(), CancellationToken.None);
                }
#pragma warning restore CS0618
                return _unfoldedCircleConfiguration;
            }
            else
            {
                _unfoldedCircleConfiguration = new UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>
                {
                    Entities = []
                };
                await using var configurationFile = File.Create(ConfigurationFilePath);
                await JsonSerializer.SerializeAsync(configurationFile,
                    _unfoldedCircleConfiguration,
                    GetSerializer(),
                    CancellationToken.None);

                return _unfoldedCircleConfiguration;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>> UpdateConfigurationAsync(UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem> configuration, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await using var configurationFileStream = File.Create(ConfigurationFilePath);
            // Do not use the cancellation token here, the file must always finish writing to ensure that the configuration is saved correctly.
            await JsonSerializer.SerializeAsync(configurationFileStream, configuration, GetSerializer(), CancellationToken.None);
            _unfoldedCircleConfiguration = configuration;
            return _unfoldedCircleConfiguration;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private DriverMetadata? _driverMetadata;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the driver.json file can't be deserialized.</exception>
    public async ValueTask<DriverMetadata> GetDriverMetadataAsync(CancellationToken cancellationToken)
    {
        if (_driverMetadata is not null)
            return _driverMetadata;

        await using var fileStream = File.OpenRead("driver.json");
        _driverMetadata = await JsonSerializer.DeserializeAsync<DriverMetadata>(fileStream, UnfoldedCircleJsonSerializerContext.Default.DriverMetadata, cancellationToken);
        return _driverMetadata ?? throw new InvalidOperationException("Failed to deserialize driver metadata");
    }

    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{T}"/> for serializing and deserializing the configuration.
    /// </summary>
    protected abstract JsonTypeInfo<UnfoldedCircleConfiguration<TGlobalConfiguration, TConfigurationItem>> GetSerializer();
}
