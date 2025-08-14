using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization.Metadata;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.DependencyInjection;
using UnfoldedCircle.Server.Event;
using UnfoldedCircle.Server.Extensions;
using UnfoldedCircle.Server.Response;

namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Handler for events and requests sent by the remote to the integration.
/// </summary>
/// <param name="configurationService">The service providing configurations.</param>
/// <param name="options">Options for customizing the behaviour of this class.</param>
/// <param name="logger">The logger used by this class.</param>
/// <typeparam name="TMediaPlayerCommandId">The type of commands used by the media player entity.</typeparam>
/// <typeparam name="TConfigurationItem">The type of configuration item the <paramref name="configurationService"/> will use.</typeparam>
public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>(
    IConfigurationService<TConfigurationItem> configurationService,
    IOptions<UnfoldedCircleOptions> options,
    ILogger<UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>> logger)
    where TMediaPlayerCommandId : struct, Enum
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    /// <summary>
    /// Service providing configurations for the integration.
    /// </summary>
    protected readonly IConfigurationService<TConfigurationItem> _configurationService = configurationService;

    /// <summary>
    /// Options for customizing the behaviour of this class.
    /// </summary>
    protected readonly IOptions<UnfoldedCircleOptions> _options = options;

    /// <summary>
    /// Logger used by this class to log messages.
    /// </summary>
    protected readonly ILogger<UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>> _logger = logger;

    /// <summary>
    /// Collection of entity types supported by this integration.
    /// </summary>
    protected abstract FrozenSet<EntityType> SupportedEntityTypes { get; }

    /// <summary>
    /// Sends <paramref name="buffer"/> to the remote client via the <paramref name="socket"/>.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send to.</param>
    /// <param name="buffer">The content to send.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected Task SendAsync(
        System.Net.WebSockets.WebSocket socket,
        ArraySegment<byte> buffer,
        string wsId,
        CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace("[{WSId}] WS: Sending message '{Message}'", wsId, Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));

        return socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    /// <summary>
    /// Maps the <paramref name="entityId"/> to the <paramref name="wsId"/>.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="entityId">The entity_id.</param>
    protected static void MapEntityIdToSocket(string wsId, string entityId) => SessionHolder.SocketIdEntityIpMap[wsId] = entityId.GetBaseIdentifier();

    /// <summary>
    /// Tries to get the entity_id from the <paramref name="wsId"/>.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="entityId">The entity_id.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    protected static bool TryGetEntityIdFromSocket(string wsId, [NotNullWhen(true)] out string? entityId)
        => SessionHolder.SocketIdEntityIpMap.TryGetValue(wsId, out entityId) && !string.IsNullOrEmpty(entityId);

    /// <summary>
    /// Removes the mapping of the <paramref name="wsId"/>.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="entityId"></param>
    protected static bool RemoveSocketFromMap(string wsId, [NotNullWhen(true)] out string? entityId)
        => SessionHolder.SocketIdEntityIpMap.TryRemove(wsId, out entityId);

    /// <summary>
    /// Marks the <paramref name="wsId"/> to receive events from the integration.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    protected static void AddSocketToEventReceivers(string wsId) => SessionHolder.SubscribeEventsMap[wsId] = true;

    /// <summary>
    /// Checks if the <paramref name="wsId"/> is subscribed to receive events.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    protected static bool IsSocketSubscribedToEvents(string wsId)
        => SessionHolder.SubscribeEventsMap.TryGetValue(wsId, out var isSubscribed) && isSubscribed;

    /// <summary>
    /// Removes the <paramref name="wsId"/> from the list of event receivers.
    /// </summary>
    /// <param name="wsId"></param>
    protected static void RemoveSocketFromEventReceivers(string wsId) => SessionHolder.SubscribeEventsMap.TryRemove(wsId, out _);

    internal async Task<WebSocketReceiveResult> HandleWebSocketAsync(
        System.Net.WebSockets.WebSocket socket,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        await SendAsync(socket,
            ResponsePayloadHelpers.CreateAuthResponsePayload(),
            wsId,
            cancellationTokenWrapper.RequestAborted);
        
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
        WebSocketReceiveResult result;
        
        do
        {
            result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("[{WSId}] WS: Received message '{Message}'", wsId, Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (result.Count == 0)
            {
                _logger.LogTrace("[{WSId}] WS: Received message is not JSON.", wsId);
                continue;
            }

            using var jsonDocument = JsonDocument.Parse(buffer.AsMemory(0, result.Count));
            if (!jsonDocument.RootElement.TryGetProperty("msg", out var msg))
            {
                _logger.LogDebug("[{WSId}] WS: Received message does not contain 'msg' property.", wsId);
                continue;
            }
            
            var messageEvent = MessageEventHelpers.GetMessageEvent(msg, out var rawValue);
            if (messageEvent == MessageEvent.Other)
            {
                _logger.LogInformation("[{WSId}] WS: Unknown message '{Message}'", wsId, rawValue);
                continue;
            }
            
            if (!jsonDocument.RootElement.TryGetProperty("kind", out var kind))
            {
                _logger.LogInformation("[{WSId}] WS: Received message does not contain 'kind' property.", wsId);
                continue;
            }
            
            if (kind.ValueEquals("req"u8))
                await HandleRequestMessage(socket, wsId, messageEvent, jsonDocument, cancellationTokenWrapper);
            else if (kind.ValueEquals("event"u8))
                await HandleEventMessage(socket, wsId, messageEvent, jsonDocument, cancellationTokenWrapper);
        } while (!result.CloseStatus.HasValue && !cancellationTokenWrapper.RequestAborted.IsCancellationRequested);

        return result;
    }

    private async ValueTask RemoveConfiguration(
        RemoveInstruction removeInstruction,
        CancellationToken cancellationToken)
    {
        var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

        var entities = configuration.Entities.Where(x => (x.DeviceId != null && string.Equals(x.DeviceId, removeInstruction.DeviceId, StringComparison.Ordinal))
                                                         || removeInstruction.EntityIds?.Contains(x.EntityId, StringComparer.OrdinalIgnoreCase) is true
                                                         || x.Host.Equals(removeInstruction.Host, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var entity in entities)
        {
            _logger.LogInformation("Removing entity {@Entity}", entity);
            configuration.Entities.Remove(entity);
        }

        await _configurationService.UpdateConfigurationAsync(configuration, cancellationToken);
    }

    private JsonTypeInfo<T>? GetCustomJsonTypeInfo<T>(in MessageEvent messageEvent)
        where T : class
        => _options.Value.MessageEventDeserializeOverrides.GetValueOrDefault(messageEvent) as JsonTypeInfo<T>;

    private record struct RemoveInstruction(string? DeviceId, IEnumerable<string>? EntityIds, string? Host);
}