using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

public sealed record SensorEntityStateChanged<TValue> : EntityStateChanged<SensorStateChangedEventMessageDataAttributes<TValue>>;
