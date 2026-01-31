namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    /// <summary>
    /// Method responsible for broadcasting updates about the given entity.
    /// </summary>
    /// <remarks>Implementor must handle <see cref="OperationCanceledException"/>.</remarks>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="subscribedEntitiesHolder">Entity ids that should receive updates.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the session.</param>
    protected internal abstract Task HandleEventUpdatesAsync(
        System.Net.WebSockets.WebSocket socket,
        string wsId,
        SubscribedEntitiesHolder subscribedEntitiesHolder,
        CancellationToken cancellationToken);
}