using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Represents a subscribed entity.
/// </summary>
/// <remarks><see cref="EntityId"/> is compared case-insensitively.</remarks>
/// <param name="EntityId">The entity_id</param>
/// <param name="EntityType">The <see cref="UnfoldedCircle.Models.Shared.EntityType"/>.</param>
// ReSharper disable once NotAccessedPositionalProperty.Global
public record SubscribedEntity(string EntityId, EntityType EntityType)
{
    /// <inheritdoc />
    public virtual bool Equals(SubscribedEntity? other) =>
        other is not null
        && EqualityContract == other.EqualityContract
        && EntityType == other.EntityType
        && string.Equals(EntityId, other.EntityId, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(EntityId), EntityType);
}
