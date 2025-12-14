using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

[EnumJsonConverter<SensorEntityAttribute>(CaseSensitive = false, PropertyName = "attributes")]
[JsonConverter(typeof(SensorEntityAttributeEntityAttributeJsonConverter))]
public enum SensorEntityAttribute : sbyte
{
    /// <summary>
    /// Optional state of the sensor.
    /// </summary>
    [Display(Name = "state")]
    State = 1,

    /// <summary>
    /// The native measurement value of the sensor.
    /// </summary>
    [Display(Name = "value")]
    Value,

    /// <summary>
    /// Optional unit of the value if no default unit is set.
    /// </summary>
    [Display(Name = "unit")]
    Unit
}