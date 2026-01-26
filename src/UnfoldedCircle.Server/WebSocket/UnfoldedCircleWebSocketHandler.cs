using System.Buffers;
using System.Collections.Frozen;
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
/// <param name="options">Options for customizing the behavior of this class.</param>
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
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly IConfigurationService<TConfigurationItem> _configurationService = configurationService;

    /// <summary>
    /// Options for customizing the behavior of this class.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly IOptions<UnfoldedCircleOptions> _options = options;

    /// <summary>
    /// Logger used by this class to log messages.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
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
    // ReSharper disable once MemberCanBePrivate.Global
    protected Task SendMessageAsync(
        System.Net.WebSockets.WebSocket socket,
        ArraySegment<byte> buffer,
        string wsId,
        CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.SendingMessage(wsId, Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));

        return socket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
    }

    /// <summary>
    /// Marks the <paramref name="wsId"/> to receive events from the integration.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    protected static void AddSocketToEventReceivers(string wsId) => SessionHolder.SubscribeEventsMap[wsId] = true;

    /// <summary>
    /// Checks if the <paramref name="wsId"/> is subscribed to receive events.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    // ReSharper disable once UnusedMember.Global
    protected static bool IsSocketSubscribedToEvents(string wsId)
        => SessionHolder.SubscribeEventsMap.TryGetValue(wsId, out var isSubscribed) && isSubscribed;

    /// <summary>
    /// Tries to add the <paramref name="wsId"/> to broadcasting events.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    // ReSharper disable once UnusedMember.Global
    protected bool TryAddSocketBroadcastingEvents(string wsId) => SessionHolder.SocketBroadcastingEvents.TryAdd(wsId, 1);

    /// <summary>
    /// Removes the <paramref name="wsId"/> from broadcasting events.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once MemberCanBePrivate.Global
    protected bool TryRemoveSocketBroadcastingEvents(string wsId) => SessionHolder.SocketBroadcastingEvents.TryRemove(wsId, out _);

    /// <summary>
    /// Marks the <paramref name="entityId"/> to receive events from the integration.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="entityId">The entity_id.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/> tied to the <paramref name="entityId"/>.</param>
    /// <returns><see langword="true"/> if the key was added, otherwise <see langword="false"/>.</returns>
    // ReSharper disable once UnusedMember.Global
    protected static bool TryAddEntityIdToBroadcastingEvents(string wsId, in ReadOnlySpan<char> entityId, CancellationTokenWrapper cancellationTokenWrapper)
    {
        cancellationTokenWrapper.AddSubscribedEntity(entityId);
        return true;
    }

    /// <summary>
    /// Removes the <paramref name="wsId"/> from the list of event receivers.
    /// </summary>
    /// <param name="wsId"></param>
    // ReSharper disable once UnusedMember.Global
    protected static void RemoveSocketFromEventReceivers(string wsId) => SessionHolder.SubscribeEventsMap.TryRemove(wsId, out _);

    internal async Task<WebSocketReceiveResult> HandleWebSocketAsync(
        System.Net.WebSockets.WebSocket socket,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateAuthResponsePayload(),
            wsId,
            cancellationTokenWrapper.RequestAborted);
        
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
        WebSocketReceiveResult result;
        
        do
        {
            result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.ReceivedMessage(wsId, Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (result.Count == 0)
            {
                _logger.NotJson(wsId);
                continue;
            }

            try
            {
                using var jsonDocument = JsonDocument.Parse(buffer.AsMemory(0, result.Count));
                if (!jsonDocument.RootElement.TryGetProperty("msg", out var msg))
                {
                    _logger.MissingMessageProperty(wsId);
                    continue;
                }

                var messageEvent = MessageEventHelpers.GetMessageEvent(msg, out var rawValue);
                if (messageEvent == MessageEvent.Other)
                {
                    _logger.UnknownMessageType(wsId, rawValue);
                    continue;
                }

                if (!jsonDocument.RootElement.TryGetProperty("kind", out var kind))
                {
                    _logger.MissingKindProperty(wsId);
                    continue;
                }

                var configuration = await _configurationService.GetConfigurationAsync(cancellationTokenWrapper.RequestAborted);
                var maxMessageHandlingWaitTimeInSeconds = configuration.MaxMessageHandlingWaitTimeInSeconds ?? _options.Value.MaxMessageHandlingWaitTimeInSeconds;
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(maxMessageHandlingWaitTimeInSeconds));
                using var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenWrapper.ApplicationStopping,
                    cancellationTokenWrapper.RequestAborted,
                    cancellationTokenSource.Token);

                if (kind.ValueEquals("req"u8))
                    await HandleRequestMessageAsync(socket, wsId, messageEvent, jsonDocument, cancellationTokenWrapper, linkedCancellationToken.Token);
                else if (kind.ValueEquals("event"u8))
                    await HandleEventMessageAsync(socket, wsId, messageEvent, jsonDocument, cancellationTokenWrapper, linkedCancellationToken.Token);
            }
            catch (Exception e)
            {
                _logger.HandleWebSocketAsyncException(wsId, e);
            }

        } while (!result.CloseStatus.HasValue && !cancellationTokenWrapper.RequestAborted.IsCancellationRequested);

        return result;
    }

    private async ValueTask RemoveConfigurationAsync(
        string wsId,
        RemoveInstruction removeInstruction,
        CancellationToken cancellationToken)
    {
        var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

        var entities = configuration.Entities.Where(x =>
                (x.DeviceId != null && string.Equals(x.DeviceId, removeInstruction.DeviceId, StringComparison.Ordinal))
                || removeInstruction.EntityIds?.Contains(x.EntityId, StringComparer.OrdinalIgnoreCase) is true
                || x.Host.Equals(removeInstruction.Host, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var entity in entities)
        {
            _logger.RemovingEntity(wsId, entity);
            configuration.Entities.Remove(entity);
        }

        await _configurationService.UpdateConfigurationAsync(configuration, cancellationToken);
    }

    private JsonTypeInfo<T>? GetCustomJsonTypeInfo<T>(in MessageEvent messageEvent)
        where T : class
        => _options.Value.MessageEventDeserializeOverrides.GetValueOrDefault(messageEvent) as JsonTypeInfo<T>;

    private record struct RemoveInstruction(string? DeviceId, IEnumerable<string>? EntityIds, string? Host);
}