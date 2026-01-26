using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Wrapper for cancellation tokens used in WebSocket handling.
/// </summary>
/// <param name="wsId">ID of the websocket.</param>
/// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send to.</param>
/// <param name="logger">The logger.</param>
/// <param name="applicationStopping">The <see cref="CancellationToken"/> used by the application to signal that the application is stopping.</param>
/// <param name="requestAborted">The <see cref="CancellationToken"/> used by the application to signal that the current request is being aborted.</param>
public sealed class CancellationTokenWrapper(
    string wsId,
    System.Net.WebSockets.WebSocket socket,
    ILogger<CancellationTokenWrapper> logger,
    in CancellationToken applicationStopping,
    in CancellationToken requestAborted) : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, sbyte> _subscribedEntities = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds an entity to the list of subscribed entities connected to this <see cref="CancellationTokenWrapper"/>.
    /// </summary>
    /// <param name="entityId">The entity_id.</param>
    /// <returns><see langowrd="true"/> if added, otherwise <see langord="false"/>.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public bool AddSubscribedEntity(in ReadOnlySpan<char> entityId)
        => _subscribedEntities.GetAlternateLookup<ReadOnlySpan<char>>().TryAdd(entityId, 0);

    /// <summary>
    /// Removes an entity from the list of subscribed entities connected to this <see cref="CancellationTokenWrapper"/>.
    /// </summary>
    /// <param name="entityId">The entity_id.</param>
    /// <returns><see langowrd="true"/> if removed, otherwise <see langord="false"/>.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public bool RemoveSubscribedEntity(in ReadOnlySpan<char> entityId)
        => _subscribedEntities.GetAlternateLookup<ReadOnlySpan<char>>().TryRemove(entityId, out _);

    /// <summary>
    /// Removes all subscribed entities.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void RemoveAllSubscribedEntities() => _subscribedEntities.Clear();

    private readonly string _wsId = wsId;
    private readonly System.Net.WebSockets.WebSocket _socket = socket;

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> that is used by the application to signal that the application is stopping.
    /// </summary>
    public readonly CancellationToken ApplicationStopping = applicationStopping;

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> that is used by the application to signal that the current request is being aborted.
    /// </summary>
    public readonly CancellationToken RequestAborted = requestAborted;

    private readonly ILogger _logger = logger;
    private CancellationTokenSource? _broadcastCancellationTokenSource;

    /// <summary>
    /// Gets the <see cref="CancellationTokenSource"/> that is used to control the cancellation of events.
    /// </summary>
    public CancellationTokenSource? GetCurrentBroadcastCancellationTokenSource() => _broadcastCancellationTokenSource;

    /// <summary>
    /// Enures that the broadcast cancellation token source is not cancelled.
    /// </summary>
    public void EnsureNonCancelledBroadcastCancellationTokenSource()
    {
        if (_broadcastCancellationTokenSource is { IsCancellationRequested: false })
            return;

        _broadcastCancellationTokenSource?.Dispose();
        _broadcastCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(RequestAborted, ApplicationStopping);
        _broadcastCancellationTokenSource.Token.Register(static callback =>
        {
            (ILogger innerLogger, ConcurrentDictionary<string, sbyte> innerSubscribedEntities) = ((ILogger, ConcurrentDictionary<string, sbyte>))callback!;
            innerLogger.BroadcastCancelled(innerSubscribedEntities);
        }, (_logger, _subscribedEntities));
    }

    private Func<System.Net.WebSockets.WebSocket, string, IReadOnlyList<string>, CancellationToken, Task>? _eventProcessor;

    /// <summary>
    /// Registers the event processor.
    /// </summary>
    /// <param name="eventProcessor">The event processor.</param>
    public void RegisterEventProcessor(Func<System.Net.WebSockets.WebSocket, string, IReadOnlyList<string>, CancellationToken, Task> eventProcessor)
        => _eventProcessor ??= eventProcessor;

    private bool _isBroadcasting;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    /// <summary>
    /// Starts event processing if not already started.
    /// </summary>
    public async ValueTask StartEventProcessing()
    {
        if (_isBroadcasting)
        {
            _logger.EventProcessingAlreadyStarted(_wsId);
            return;
        }

        if (!await _semaphoreSlim.WaitAsync(TimeSpan.FromMilliseconds(100), RequestAborted))
        {
            _logger.EventProcessingStartTimeout(_wsId);
            return;
        }

        try
        {
            if (_isBroadcasting)
            {
                _logger.EventProcessingAlreadyStarted(_wsId);
                return;
            }

            _isBroadcasting = true;
            EnsureNonCancelledBroadcastCancellationTokenSource();
            // Fire and forget, logging happens in the invoked method
            _ = _eventProcessor?.Invoke(_socket, _wsId, (IReadOnlyList<string>)_subscribedEntities.Keys, _broadcastCancellationTokenSource!.Token);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Attempts to stop event processing.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public async ValueTask StopEventProcessingAsync()
    {
        if (!_isBroadcasting)
        {
            _logger.EventProcessingNotStarted(_wsId);
            return;
        }

        if (_broadcastCancellationTokenSource != null)
            await _broadcastCancellationTokenSource.CancelAsync();
    }

    private bool _isDisposed;

    void IDisposable.Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, true, _isDisposed))
            return;

        _subscribedEntities.Clear();
        _semaphoreSlim.Dispose();
        _broadcastCancellationTokenSource?.Dispose();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, true, _isDisposed))
            return;

        _subscribedEntities.Clear();
        _semaphoreSlim.Dispose();
        if (_broadcastCancellationTokenSource != null)
        {
            await _broadcastCancellationTokenSource.CancelAsync();
        }
    }
}
