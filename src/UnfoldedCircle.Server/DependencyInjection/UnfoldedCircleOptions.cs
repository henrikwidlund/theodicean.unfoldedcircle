using System.Globalization;
using System.Text.Json.Serialization.Metadata;

using UnfoldedCircle.Server.Event;

namespace UnfoldedCircle.Server.DependencyInjection;

/// <summary>
/// Options for configuring the Unfolded Circle server.
/// </summary>
public class UnfoldedCircleOptions
{
    /// <summary>
    /// Set to true if you want entity_ids and device_ids to be returned without a prefix.
    /// This should only be used if your integration has devices setup before prefixing was introduced.
    /// </summary>
    /// <remarks>
    /// This will disable the prefixes for all entity types and will cause duplicates if your integration supports multiple entity types.
    /// Only read at startup.
    /// </remarks>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool DisableEntityIdPrefixing { get; set; }

    /// <summary>
    /// The default port to listen to for incoming connections.
    /// <remarks>
    /// This setting will only be used if the <c>UC_INTEGRATION_HTTP_PORT</c> environment variable is not set
    /// and is only read at startup.
    /// Default value is <c>UC_INTEGRATION_HTTP_PORT</c> -> <c>ASPNETCORE_URLS</c> -> <c>9001</c>
    /// </remarks>
    /// </summary>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public ushort ListeningPort { get; set; } = GetListeningPort();

    /// <summary>
    /// Custom deserialization overrides for specific <see cref="MessageEvent"/> types.
    /// </summary>
    public Dictionary<MessageEvent, JsonTypeInfo> MessageEventDeserializeOverrides
    {
        get => field ??= [];
        // ReSharper disable once UnusedMember.Global
        set;
    }

    /// <summary>
    /// The maximum wait time for a received message to be handled before being cancelled.
    /// </summary>
    /// <remarks>Default is 9.5 seconds.</remarks>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public double MaxMessageHandlingWaitTimeInSeconds { get; set; } = 9.5;

    private static ushort GetListeningPort()
    {
        var envPort = Environment.GetEnvironmentVariable("UC_INTEGRATION_HTTP_PORT");
        if (ushort.TryParse(envPort, NumberFormatInfo.InvariantInfo, out var port))
            return port;

        envPort = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (!string.IsNullOrEmpty(envPort))
        {
            var span = envPort.AsSpan();
            span = span.Slice(span.LastIndexOf(':') + 1).Trim('/');
            if (ushort.TryParse(span, NumberFormatInfo.InvariantInfo, out port))
                return port;
        }

        return 9001;
    }
}