using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

public sealed record SelectEntityStateChanged : EntityStateChanged<SelectStateChangedEventMessageDataAttributes>;
