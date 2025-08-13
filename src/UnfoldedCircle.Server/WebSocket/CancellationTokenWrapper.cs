using Microsoft.Extensions.Logging;

namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Wrapper for cancellation tokens used in WebSocket handling.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="applicationStopping">The <see cref="CancellationToken"/> used by the application to signal that the application is stopping.</param>
/// <param name="requestAborted">The <see cref="CancellationToken"/> used by the application to signal that the current request is being aborted.</param>
public sealed class CancellationTokenWrapper(
    ILogger<CancellationTokenWrapper> logger,
    in CancellationToken applicationStopping,
    in CancellationToken requestAborted) : IDisposable
{
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
        _broadcastCancellationTokenSource.Token.Register(static logger => ((ILogger)logger!).LogWarning("Broadcast cancelled"), _logger);
    }

    /// <inheritdoc />
    public void Dispose() => _broadcastCancellationTokenSource?.Dispose();
}