using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Events;

public sealed record DisconnectEvent : CommonEventOptional<DeviceIdObject>;
