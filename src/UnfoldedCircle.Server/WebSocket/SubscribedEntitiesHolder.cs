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
    internal void AddSubscribedEntity(string entityId)
    {
        var baseIdentifier = entityId.GetBaseIdentifier();
        var entityType = entityId.GetEntityTypeFromIdentifier();
        _subscribedEntities.AddOrUpdate(baseIdentifier, static (_, arg)
            => [new SubscribedEntity(arg.entityId, arg.entityType)],
            static (_, set, arg) => [.. set, new SubscribedEntity(arg.entityId, arg.entityType)],
            (entityId, entityType));
    }

    /// <summary>
    /// Removes a subscribed entity.
    /// </summary>
    /// <param name="entityId">Entity to remove.</param>
    internal void RemoveSubscribedEntity(string entityId)
    {
        var baseIdentifier = entityId.GetBaseIdentifier();
        while (_subscribedEntities.TryGetValue(baseIdentifier, out var current))
        {
            HashSet<SubscribedEntity>? updated = null;
            foreach (var subscribedEntity in current)
            {
                if (!subscribedEntity.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase))
                    (updated ??= new HashSet<SubscribedEntity>(current.Count)).Add(subscribedEntity);
            }

            updated ??= [];
            if (updated.Count == current.Count)
                return;

            if (_subscribedEntities.TryUpdate(baseIdentifier, updated, current))
                return;
        }
    }

    internal void Clear() => _subscribedEntities.Clear();
}
