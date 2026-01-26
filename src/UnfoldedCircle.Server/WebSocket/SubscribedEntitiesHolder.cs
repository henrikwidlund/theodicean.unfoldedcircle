namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Holds record of subscribed entities.
/// </summary>
public class SubscribedEntitiesHolder
{
    private readonly HashSet<string> _subscribedEntities = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// List of subscribed entities.
    /// </summary>
    public IReadOnlySet<string> SubscribedEntities => _subscribedEntities;

    /// <summary>
    /// Adds a subscribed entity.
    /// </summary>
    /// <param name="entityId">Entity to add.</param>
    internal bool AddSubscribedEntity(in ReadOnlySpan<char> entityId)
        => _subscribedEntities.GetAlternateLookup<ReadOnlySpan<char>>().Add(entityId);

    /// <summary>
    /// Removes a subscribed entity.
    /// </summary>
    /// <param name="entityId">Entity to remove.</param>
    internal bool RemoveSubscribedEntity(in ReadOnlySpan<char> entityId)
        => _subscribedEntities.GetAlternateLookup<ReadOnlySpan<char>>().Remove(entityId);

    internal void Clear() => _subscribedEntities.Clear();
}