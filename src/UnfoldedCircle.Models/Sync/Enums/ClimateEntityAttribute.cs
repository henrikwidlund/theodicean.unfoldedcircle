using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

[EnumJsonConverter<ClimateEntityAttribute>(CaseSensitive = false, PropertyName = "attributes")]
[JsonConverter(typeof(ClimateEntityAttributeJsonConverter))]
public enum ClimateEntityAttribute : sbyte
{
    /// <summary>
    /// State of the climate device, corresponds to <see cref="HvacMode"/>.
    /// </summary>
    [Display(Name = "state")]
    State = 1,

    [Display(Name = "current_temperature")]
    CurrentTemperature,

    [Display(Name = "target_temperature")]
    TargetTemperature
}