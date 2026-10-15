namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<DriverSetupChangeState>))]
public enum DriverSetupChangeState : byte
{
    /// <summary>
    /// setup in progress
    /// </summary>
    [JsonStringEnumMemberName("SETUP")]
    Setup = 1,

    /// <summary>
    /// setup flow is waiting for user input. See require_user_action for requested input.
    /// </summary>
    [JsonStringEnumMemberName("WAIT_USER_ACTION")]
    WaitUserAction,

    /// <summary>
    /// setup finished successfully
    /// </summary>
    [JsonStringEnumMemberName("OK")]
    Ok,

    /// <summary>
    /// setup error
    /// </summary>
    [JsonStringEnumMemberName("ERROR")]
    Error
}
