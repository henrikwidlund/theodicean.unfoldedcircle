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
    /// The delay before starting to broadcast events after event broadcasting is initiated
    /// </summary>
    /// <remarks>Defaults to 1s.</remarks>
    public TimeSpan DelayBeforeStartEventBroadcasting
    {
        get;
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global, UnusedMember.Global
        set
        {
            if (value <= TimeSpan.Zero)
                throw new InvalidOperationException("Value must be greater than zero.");
            field = value;
        }
    } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The maximum wait time for a received message to be handled before being canceled.
    /// </summary>
    /// <remarks>
    /// Default is 9.5 seconds. The remote terminates the request if no response is received within 10 seconds,
    /// so this value is set just below that threshold to ensure the handler is canceled and an error response
    /// can still be sent before the remote times out. Setup messages (<c>setup_driver</c> and
    /// <c>set_driver_user_data</c>) bypass this limit entirely because the remote uses a separate setup-flow
    /// timer for those. For handlers that may genuinely exceed this duration (e.g. long IR repeat sequences),
    /// the recommended pattern is to acknowledge the request early before performing the slow work, as done
    /// in the entity command repeat/sequence paths.
    /// </remarks>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public double MaxMessageHandlingWaitTimeInSeconds { get; set; } = 9.5;

    /// <summary>
    /// Additional JSON property names whose scalar value should be masked when WebSocket frames are
    /// logged. Matched case-sensitively.
    /// </summary>
    /// <remarks>Only read at startup.</remarks>
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public ICollection<string> AdditionalRedactedJsonProperties { get; set; } = [];

    /// <summary>
    /// Additional JSON property names whose entire value (including nested objects and arrays) should be
    /// replaced with a single mask when WebSocket frames are logged. Matched case-sensitively.
    /// </summary>
    /// <remarks>Only read at startup.</remarks>
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public ICollection<string> AdditionalMaskWholeValueJsonProperties { get; set; } = [];

    /// <summary>
    /// When <see langword="true"/> (default), the trace-log redactor will attempt to parse string values that look
    /// like JSON (begin with <c>{</c>) and recursively redact sensitive properties inside them. Catches
    /// secrets embedded in stringified JSON payloads (e.g. setup-flow textarea blobs) that are not covered
    /// by an explicit whole-value mask. Set to <see langword="false"/> to disable if it ever causes measurable cost
    /// on the embedded host.
    /// </summary>
    /// <remarks>Only read at startup.</remarks>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public bool EnableNestedJsonRedaction { get; set; } = true;

    /// <summary>
    /// Maximum recursion depth for nested-JSON-string redaction. Bounds the cost of pathologically nested
    /// stringified payloads. Default is <c>3</c>.
    /// </summary>
    /// <remarks>Only read at startup.</remarks>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public int MaxJsonRedactionRecursionDepth { get; set; } = 3;

    private static ushort GetListeningPort()
    {
        var envPort = Environment.GetEnvironmentVariable("UC_INTEGRATION_HTTP_PORT");
        if (ushort.TryParse(envPort, NumberFormatInfo.InvariantInfo, out var port))
            return port;

        envPort = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (!string.IsNullOrEmpty(envPort))
        {
            var span = envPort.AsSpan();
            span = span[(span.LastIndexOf(':') + 1)..].Trim('/');
            if (ushort.TryParse(span, NumberFormatInfo.InvariantInfo, out port))
                return port;
        }

        return 9001;
    }
}
