using System.Collections.Concurrent;

using UnfoldedCircle.Server.Extensions;

namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Holds record of subscribed entities.
/// </summary>
public class SubscribedEntitiesHolder
{
    private readonly ConcurrentDictionary<string, HashSet<SubscribedEntity>> _subscribedEntities = [];

    /// <summary>
    /// List of subscribed entities.
    /// </summary>
    public IReadOnlyDictionary<string, HashSet<SubscribedEntity>> SubscribedEntities => _subscribedEntities;

    /// <summary>
    /// Adds a subscribed entity.
    /// </summary>
    /// <param name="entityId">Entity to add.</param>
    internal bool AddSubscribedEntity(string entityId)
    {
        var baseIdentifier = entityId.GetBaseIdentifier();
        var entityType = entityId.GetEntityTypeFromIdentifier();
        _subscribedEntities.AddOrUpdate(baseIdentifier, static (_, arg)
            => [new SubscribedEntity(arg.entityId, arg.entityType)],
            static (_, set, arg) =>
        {
            set.Add(new SubscribedEntity(arg.entityId, arg.entityType));
            return set;
        }, (entityId, entityType));
        return true;
    }

    /// <summary>
    /// Removes a subscribed entity.
    /// </summary>
    /// <param name="entityId">Entity to remove.</param>
    internal bool RemoveSubscribedEntity(string entityId)
    {
        var baseIdentifier = entityId.AsSpan().GetBaseIdentifier();
        var alternateLookup = _subscribedEntities.GetAlternateLookup<ReadOnlySpan<char>>();
        if (alternateLookup.TryGetValue(baseIdentifier, out var subscribedEntities))
            subscribedEntities.RemoveWhere(e => e.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
        return true;
    }

    internal void Clear() => _subscribedEntities.Clear();
}