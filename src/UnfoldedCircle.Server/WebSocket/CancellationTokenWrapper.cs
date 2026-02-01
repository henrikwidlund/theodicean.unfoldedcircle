using System.Diagnostics.CodeAnalysis;

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
    private readonly SubscribedEntitiesHolder _subscribedEntities = new();

    /// <summary>
    /// Adds an entity to the list of subscribed entities connected to this <see cref="CancellationTokenWrapper"/>.
    /// </summary>
    /// <param name="entityId">The entity_id.</param>
    /// <returns><see langowrd="true"/> if added, otherwise <see langord="false"/>.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    // ReSharper disable once UnusedMember.Global
    public void AddSubscribedEntity(string entityId)
        => _subscribedEntities.AddSubscribedEntity(entityId);

    /// <summary>
    /// Removes an entity from the list of subscribed entities connected to this <see cref="CancellationTokenWrapper"/>.
    /// </summary>
    /// <param name="entityId">The entity_id.</param>
    /// <returns><see langowrd="true"/> if removed, otherwise <see langord="false"/>.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    // ReSharper disable once UnusedMember.Global
    public void RemoveSubscribedEntity(string entityId)
        => _subscribedEntities.RemoveSubscribedEntity(entityId);

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

    [MemberNotNull(nameof(_broadcastCancellationTokenSource))]
    private void EnsureNonCancelledBroadcastCancellationTokenSourceCore()
    {
        if (_broadcastCancellationTokenSource is { IsCancellationRequested: false })
            return;

        _broadcastCancellationTokenSource?.Dispose();
        _broadcastCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(RequestAborted, ApplicationStopping);
        _broadcastCancellationTokenSource.Token.Register(static callback =>
        {
            (ILogger innerLogger, SubscribedEntitiesHolder innerSubscribedEntities) = ((ILogger, SubscribedEntitiesHolder))callback!;
            innerLogger.BroadcastCancelled(innerSubscribedEntities.SubscribedEntities.Keys);
        }, (_logger, _subscribedEntities));
    }

    private Func<System.Net.WebSockets.WebSocket, string, SubscribedEntitiesHolder, CancellationToken, Task>? _eventProcessor;

    /// <summary>
    /// Registers the event processor.
    /// </summary>
    /// <param name="eventProcessor">The event processor.</param>
    public void RegisterEventProcessor(Func<System.Net.WebSockets.WebSocket, string, SubscribedEntitiesHolder, CancellationToken, Task> eventProcessor)
        => _eventProcessor ??= eventProcessor;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private Task? _broadcastTask;

    /// <summary>
    /// Starts event processing if not already started.
    /// </summary>
    public async ValueTask StartEventProcessingAsync()
    {
        if (await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(1), RequestAborted))
        {
            try
            {
                if (_eventProcessor == null)
                {
                    _logger.EventProcessorNotRegistered(_wsId);
                    return;
                }

                if (_broadcastTask is not null)
                {
                    if (_broadcastTask.IsFaulted)
                        _logger.UnhandledExceptionDuringEvent(_wsId, _broadcastTask.Exception.GetBaseException());

                    _logger.ResettingEventProcessing(_wsId, _broadcastTask.Status);
                    await (_broadcastCancellationTokenSource?.CancelAsync() ?? Task.CompletedTask);
                }

                EnsureNonCancelledBroadcastCancellationTokenSourceCore();
                _broadcastTask = _eventProcessor.Invoke(_socket, _wsId, _subscribedEntities, _broadcastCancellationTokenSource.Token);
                return;
            }
            catch (Exception ex)
            {
                _logger.UnhandledExceptionDuringStartEvent(_wsId, ex);
                _broadcastTask = null;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        _logger.StartEventProcessorSemaphoreTimeout(_wsId);
    }

    /// <summary>
    /// Attempts to stop event processing.
    /// </summary>
    public async ValueTask StopEventProcessingAsync()
    {
        if (await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(1), RequestAborted))
        {
            try
            {
                if (_broadcastCancellationTokenSource != null)
                    await _broadcastCancellationTokenSource.CancelAsync();

                if (_broadcastTask is { IsFaulted: true })
                    _logger.UnhandledExceptionDuringEvent(_wsId, _broadcastTask.Exception.GetBaseException());
                _broadcastTask = null;
                return;
            }
            catch (Exception ex)
            {
                _logger.UnhandledExceptionDuringStopEvent(_wsId, ex);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        _logger.StopEventProcessorSemaphoreTimeout(_wsId);
    }

    private bool _isDisposed;

    void IDisposable.Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, true))
            return;

        _subscribedEntities.Clear();
        _semaphoreSlim.Dispose();
        _broadcastCancellationTokenSource?.Dispose();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, true))
            return;

        _subscribedEntities.Clear();
        _semaphoreSlim.Dispose();
        if (_broadcastCancellationTokenSource != null)
            await _broadcastCancellationTokenSource.CancelAsync();
    }
}
