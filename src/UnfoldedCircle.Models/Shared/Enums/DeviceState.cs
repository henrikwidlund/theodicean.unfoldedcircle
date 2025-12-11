using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Shared;

[EnumJsonConverter<DeviceState>(CaseSensitive = false, PropertyName = "state")]
[JsonConverter(typeof(DeviceStateJsonConverter))]
public enum DeviceState : sbyte
{
    [Display(Name = "CONNECTED")]
    Connected = 1,
    
    [Display(Name = "CONNECTING")]
    Connecting,
    
    [Display(Name = "DISCONNECTED")]
    Disconnected,
    
    [Display(Name = "ERROR")]
    Error
}
