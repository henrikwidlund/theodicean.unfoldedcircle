namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<DriverSetupChangeEventType>))]
public enum DriverSetupChangeEventType : byte
{
    /// <summary>
    /// setup started
    /// </summary>
    [JsonStringEnumMemberName("START")]
    Start = 1,

    /// <summary>
    /// setup in progress. See <see cref="DriverSetupChangeState"/> enum for current setup state.
    /// </summary>
    [JsonStringEnumMemberName("SETUP")]
    Setup,

    /// <summary>
    /// setup finished, either with: state: <see cref="DriverSetupChangeState.Ok"/> for successful setup, or state: <see cref="DriverSetupChangeState.Error"/> if setup didn't complete successfully.
    /// </summary>
    [JsonStringEnumMemberName("STOP")]
    Stop
}
