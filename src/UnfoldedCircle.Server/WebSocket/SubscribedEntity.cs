using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Server.WebSocket;

/// <summary>
/// Represents a subscribed entity.
/// </summary>
/// <param name="EntityId">The entity_id</param>
/// <param name="EntityType">The <see cref="UnfoldedCircle.Models.Shared.EntityType"/>.</param>
// ReSharper disable once NotAccessedPositionalProperty.Global
public record SubscribedEntity(string EntityId, in EntityType EntityType);