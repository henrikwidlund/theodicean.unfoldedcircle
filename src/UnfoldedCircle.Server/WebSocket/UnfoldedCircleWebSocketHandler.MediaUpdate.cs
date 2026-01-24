namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    /// <summary>
    /// Method responsible for broadcasting updates about the given entity.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/> for the session.</param>
    protected abstract Task HandleEventUpdatesAsync(
        System.Net.WebSockets.WebSocket socket,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper);
}