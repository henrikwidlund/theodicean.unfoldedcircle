using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Sync;

public sealed record GetDeviceStateMsg : CommonReq<DeviceIdObject>;
