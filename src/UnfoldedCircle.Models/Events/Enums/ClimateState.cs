namespace UnfoldedCircle.Models.Events;

/// <remarks>
/// The current mode may not be the active state of the device.
/// E.g. if the mode is set to <see cref="Auto"/> the climate unit may be heating, cooling, idle, etc. at a specific point in time.
/// </remarks>
[JsonConverter(typeof(StrictEnumConverter<ClimateState>))]
public enum ClimateState : byte
{
    /// <summary>
    /// The climate device is switched off.
    /// </summary>
    [JsonStringEnumMemberName("OFF")]
    Off,

    /// <summary>
    /// The device is set to heating, optionally to a set target temperature.
    /// </summary>
    [JsonStringEnumMemberName("HEAT")]
    Heat,

    /// <summary>
    /// The device is set to cooling, optionally to a set target temperature.
    /// </summary>
    [JsonStringEnumMemberName("COOL")]
    Cool,

    /// <summary>
    /// The device is set to heat or cool to a target temperature range.
    /// </summary>
    [JsonStringEnumMemberName("HEAT_COOL")]
    HeatCool,

    /// <summary>
    /// Fan-only mode without heating or cooling.
    /// </summary>
    [JsonStringEnumMemberName("FAN")]
    Fan,

    /// <summary>
    /// The device is set to automatic mode. This is device dependent, e.g. according to a schedule, presence detection, etc.
    /// </summary>
    [JsonStringEnumMemberName("AUTO")]
    Auto,

    /// <summary>
    /// The entity is currently not available. The UI will render the entity as inactive until the entity becomes active again.
    /// </summary>
    [JsonStringEnumMemberName("UNAVAILABLE")]
    Unavailable,

    /// <summary>
    /// The entity is available but the current state is unknown.
    /// </summary>
    [JsonStringEnumMemberName("UNKNOWN")]
    Unknown
}
