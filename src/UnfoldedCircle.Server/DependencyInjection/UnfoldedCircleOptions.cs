using System.Text.Json.Serialization.Metadata;

using UnfoldedCircle.Server.Event;

namespace UnfoldedCircle.Server.DependencyInjection;

/// <summary>
/// Options for configuring the Unfolded Circle server.
/// </summary>
public class UnfoldedCircleOptions
{
    private Dictionary<MessageEvent, JsonTypeInfo>? _messageEventDeserializeOverrides;

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
    /// and is only read at startup.</remarks>
    /// </summary>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public int ListeningPort { get; set; } = 9001;

    /// <summary>
    /// Custom deserialization overrides for specific <see cref="MessageEvent"/> types.
    /// </summary>
    public Dictionary<MessageEvent, JsonTypeInfo> MessageEventDeserializeOverrides
    {
        get => _messageEventDeserializeOverrides ??= [];
        // ReSharper disable once UnusedMember.Global
        set => _messageEventDeserializeOverrides = value;
    }
}