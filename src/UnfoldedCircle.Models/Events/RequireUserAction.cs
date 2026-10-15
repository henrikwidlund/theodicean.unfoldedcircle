using UnfoldedCircle.Models.Sync;

namespace UnfoldedCircle.Models.Events;

public sealed record RequireUserAction
{
    [JsonPropertyName("input")]
    public SettingsPage? Input { get; init; }

    [JsonPropertyName("confirmation")]
    public ConfirmationPage? Confirmation { get; init; }
}
