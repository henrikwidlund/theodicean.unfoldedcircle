using System.Text.Json.Serialization.Metadata;

using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Json;

namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Base class for configuration services that manage Unfolded Circle configurations.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/> used to determine the directory where files are stored.</param>
/// <typeparam name="TConfigurationItem">The type used for storing entity information.</typeparam>
/// <remarks>
/// Integration driver metadata is read from <c>driver.json</c> in the same directory as the program's executable.
/// Entity settings are stored in <c>configured_entities.json</c>, located in the <c>UC_CONFIG_HOME</c>,
/// or the same folder as the driver.json file if empty.
/// </remarks>
// ReSharper disable once UnusedType.Global
public abstract class ConfigurationService<TConfigurationItem>(IConfiguration configuration) : IConfigurationService<TConfigurationItem>
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    private readonly IConfiguration _configuration = configuration;
    private string? _ucConfigHome;
    private string UcConfigHome => _ucConfigHome ??= _configuration["UC_CONFIG_HOME"] ?? string.Empty;
    private string ConfigurationFilePath => Path.Combine(UcConfigHome, "configured_entities.json");
    private UnfoldedCircleConfiguration<TConfigurationItem>? _unfoldedCircleConfiguration;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the configuration file can't be deserialized.</exception>
    public async Task<UnfoldedCircleConfiguration<TConfigurationItem>> GetConfigurationAsync(CancellationToken cancellationToken)
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
                return _unfoldedCircleConfiguration;
            }
            else
            {
                _unfoldedCircleConfiguration = new UnfoldedCircleConfiguration<TConfigurationItem>
                {
                    Entities = []
                };
                await using var configurationFile = File.Create(ConfigurationFilePath);
                await JsonSerializer.SerializeAsync(configurationFile,
                    _unfoldedCircleConfiguration,
                    GetSerializer(),
                    cancellationToken);
                
                return _unfoldedCircleConfiguration;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<UnfoldedCircleConfiguration<TConfigurationItem>> UpdateConfigurationAsync(UnfoldedCircleConfiguration<TConfigurationItem> configuration, CancellationToken cancellationToken)
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
    protected abstract JsonTypeInfo<UnfoldedCircleConfiguration<TConfigurationItem>> GetSerializer();
}