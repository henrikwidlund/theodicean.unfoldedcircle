using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<ClimateFeature>(CaseSensitive = false, PropertyName = "features")]
[JsonConverter(typeof(ClimateFeaturesJsonConverter))]
public enum ClimateFeature : sbyte
{
    /// <summary>
    /// The device can be turned on and off.
    /// The active HVAC mode after power on is device specific and must be reflected in the state attribute.
    /// </summary>
    [Display(Name = "on_off")]
    OnOff,

    /// <summary>
    /// The device supports heating.
    /// </summary>
    [Display(Name = "heat")]
    Heat,

    /// <summary>
    /// The device supports cooling.
    /// </summary>
    [Display(Name = "cool")]
    Cool,

    /// <summary>
    /// The device can measure the current temperature.
    /// </summary>
    [Display(Name = "current_temperature")]
    CurrentTemperature,

    /// <summary>
    /// The device supports a target temperature for heating or cooling.
    /// </summary>
    [Display(Name = "target_temperature")]
    TargetTemperature,

    /// <summary>
    /// The device supports a target temperature range.
    /// </summary>
    [Display(Name = "target_temperature_range")]
    TargetTemperatureRange,

    /// <summary>
    /// The device has a controllable fan.
    /// </summary>
    [Display(Name = "fan")]
    Fan
}