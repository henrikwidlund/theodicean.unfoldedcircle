using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<SensorState>(CaseSensitive = false, PropertyName = "state")]
[JsonConverter(typeof(SensorStateJsonConverter))]
public enum SensorState
{
    [Display(Name = "ON")]
    On,

    [Display(Name = "UNAVAILABLE")]
    Unavailable,

    [Display(Name = "UNKNOWN")]
    Unknown
}