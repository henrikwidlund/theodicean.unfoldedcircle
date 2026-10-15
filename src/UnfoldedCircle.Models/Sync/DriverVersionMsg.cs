namespace UnfoldedCircle.Models.Sync;

/// <summary>
/// Get version information about the integration driver.
/// </summary>
public sealed record DriverVersionMsg : CommonRespRequired<DriverVersion>;
