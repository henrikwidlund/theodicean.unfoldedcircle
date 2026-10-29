using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

public sealed record ClimateEntityStateChanged : EntityStateChanged<ClimateStateChangedEventMessageDataAttributes>;
