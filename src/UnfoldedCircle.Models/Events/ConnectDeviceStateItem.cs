using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Events;

public sealed record ConnectDeviceStateItem
{
    [JsonPropertyName("state")]
    public required DeviceState State { get; init; }
}
