namespace UnfoldedCircle.Models.Events;

public sealed record StateChangedEvent<TAttributes> : CommonEventRequired<StateChangedEventMessageData<TAttributes>>
    where TAttributes : StateChangedEventMessageDataAttributes;
