using Makaretu.Dns;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.DependencyInjection;

namespace UnfoldedCircle.Server.Dns;

internal sealed class MDnsBackgroundService<TConfigurationItem>(
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    IConfigurationService<TConfigurationItem> configurationService,
    IOptions<UnfoldedCircleOptions> unfoldedCircleOptions)
    : IHostedService, IDisposable
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly IConfigurationService<TConfigurationItem> _configurationService = configurationService;
    private readonly IOptions<UnfoldedCircleOptions> _unfoldedCircleOptions = unfoldedCircleOptions;
    private ServiceProfile? _serviceProfile;
    private ServiceDiscovery? _serviceDiscovery;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var driverMetadata = await _configurationService.GetDriverMetadataAsync(cancellationToken);
        // Get the local hostname
        _serviceProfile = new ServiceProfile(driverMetadata.DriverId,
            "_uc-integration._tcp",
            _configuration.GetOrDefault("UC_INTEGRATION_HTTP_PORT", _unfoldedCircleOptions.Value.ListeningPort))
        {
            HostName = $"{System.Net.Dns.GetHostName().Split('.')[0]}.local"
        };

        // Add TXT records
        _serviceProfile.AddProperty("name", driverMetadata.Name["en"]);
        _serviceProfile.AddProperty("ver", driverMetadata.Version);
        _serviceProfile.AddProperty("developer", driverMetadata.Developer?.Name ?? "N/A");
        _serviceDiscovery = await ServiceDiscovery.CreateInstance(loggerFactory: _loggerFactory, cancellationToken: cancellationToken);
        _serviceDiscovery.Advertise(_serviceProfile);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serviceProfile is not null && _serviceDiscovery is not null)
            await _serviceDiscovery.Unadvertise(_serviceProfile);
    }

    public void Dispose() => _serviceDiscovery?.Dispose();
}