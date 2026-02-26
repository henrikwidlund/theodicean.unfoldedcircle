using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

[EnumJsonConverter<ClimateCommandId>(CaseSensitive = false, PropertyName = "cmd_id")]
[JsonConverter(typeof(ClimateCommandIdJsonConverter))]
public enum ClimateCommandId
{
    /// <summary>
    /// Switch on the climate device.
    /// </summary>
    [Display(Name = "on")]
    On,

    /// <summary>
    /// Switch off the climate device.
    /// </summary>
    [Display(Name = "off")]
    Off,

    /// <summary>
    /// Set the device to heating, cooling, etc.
    /// </summary>
    /// <see cref="ClimateState"/>
    [Display(Name = "hvac_mode")]
    HvacMode,

    /// <summary>
    /// Change the target temperature
    /// </summary>
    [Display(Name = "target_temperature")]
    TargetTemperature,

    [Display(Name = "target_temperature_range")]
    TargetTemperatureRange,

    [Display(Name = "fan_mode")]
    FanMode
}