using System.Text.Json.Serialization.Metadata;

using UnfoldedCircle.Server.Event;

namespace UnfoldedCircle.Server.DependencyInjection;

/// <summary>
/// Options for configuring the Unfolded Circle server.
/// </summary>
public class UnfoldedCircleOptions
{
    /// <summary>
    /// The default port to listen to for incoming connections.
    /// <remarks>This setting will only be used if the <c>UC_INTEGRATION_HTTP_PORT</c> environment variable is not set.</remarks>
    /// </summary>
    public int ListeningPort { get; set; } = 9001;

    /// <summary>
    /// Custom deserialization overrides for specific <see cref="MessageEvent"/> types.
    /// </summary>
    public Dictionary<MessageEvent, JsonTypeInfo> MessageEventDeserializeOverrides { get; } = new();
}