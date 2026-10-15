namespace UnfoldedCircle.Models.Events;

public sealed record UserInterface
{
    [JsonPropertyName("pages")]
    public UserInterfacePage[]? Pages { get; init; }
}
