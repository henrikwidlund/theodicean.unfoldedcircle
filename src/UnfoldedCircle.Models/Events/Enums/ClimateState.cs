using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

/// <remarks>
/// The current mode may not be the active state of the device.
/// E.g. if the mode is set to <see cref="Auto"/> the climate unit may be heating, cooling, idle, etc. at a specific point in time.
/// </remarks>
[EnumJsonConverter<ClimateState>(CaseSensitive = false, PropertyName = "state")]
[JsonConverter(typeof(ClimateStateJsonConverter))]
public enum ClimateState
{
    /// <summary>
    /// The climate device is switched off.
    /// </summary>
    [Display(Name = "OFF")]
    Off,

    /// <summary>
    /// The device is set to heating, optionally to a set target temperature.
    /// </summary>
    [Display(Name = "HEAT")]
    Heat,

    /// <summary>
    /// The device is set to cooling, optionally to a set target temperature.
    /// </summary>
    [Display(Name = "COOL")]
    Cool,

    /// <summary>
    /// The device is set to heat or cool to a target temperature range.
    /// </summary>
    [Display(Name = "HEAT_COOL")]
    HeatCool,

    /// <summary>
    /// Fan-only mode without heating or cooling.
    /// </summary>
    [Display(Name = "FAN")]
    Fan,

    /// <summary>
    /// The device is set to automatic mode. This is device dependant, e.g. according to a schedule, presence detection, etc.
    /// </summary>
    [Display(Name = "AUTO")]
    Auto,

    /// <summary>
    /// The entity is currently not available. The UI will render the entity as inactive until the entity becomes active again.
    /// </summary>
    [Display(Name = "UNAVAILABLE")]
    Unavailable,

    /// <summary>
    /// The entity is available but the current state is unknown.
    /// </summary>
    [Display(Name = "UNKNOWN")]
    Unknown
}