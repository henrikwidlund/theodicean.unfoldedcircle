using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<State>(CaseSensitive = false, PropertyName = "state")]
[JsonConverter(typeof(StateJsonConverter))]
public enum State : sbyte
{
    [Display(Name = "UNAVAILABLE")]
    Unavailable,
    
    [Display(Name = "UNKNOWN")]
    Unknown,
    
    [Display(Name = "ON")]
    On,
    
    [Display(Name = "OFF")]
    Off,
    
    [Display(Name = "PLAYING")]
    Playing,
    
    [Display(Name = "PAUSED")]
    Paused,
    
    [Display(Name = "STANDBY")]
    Standby,
    
    [Display(Name = "BUFFERING")]
    Buffering
}
