using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<TemperatureUnit>(CaseSensitive = false, PropertyName = "temperature_unit")]
[JsonConverter(typeof(TemperatureUnitJsonConverter))]
public enum TemperatureUnit
{
    /// <summary>
    /// Celsius
    /// </summary>
    [Display(Name = "CELSIUS")]
    Celsius,

    /// <summary>
    /// Fahrenheit
    /// </summary>
    [Display(Name = "FAHRENHEIT")]
    Fahrenheit
}