using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Shared;

[EnumJsonConverter<RepeatMode>(CaseSensitive = false, PropertyName = "repeat")]
[JsonConverter(typeof(RepeatModeJsonConverter))]
public enum RepeatMode : sbyte
{
    [Display(Name = "OFF")]
    Off = 1,
    
    [Display(Name = "ALL")]
    All,
    
    [Display(Name = "ONE")]
    One
}
