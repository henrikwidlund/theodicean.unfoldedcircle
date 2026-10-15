using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Events;

public sealed record ConnectEvent : CommonEventOptional<DeviceIdObject>;
