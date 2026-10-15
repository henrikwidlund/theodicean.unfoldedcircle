namespace UnfoldedCircle.Models.Events;

public sealed record RemoteStateChangedEventMessageDataAttributes : StateChangedEventMessageDataAttributes
{
    [JsonPropertyName("state")]
    public required RemoteState State { get; init; }
}
