using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<RemoteState>(CaseSensitive = false, PropertyName = "state")]
[JsonConverter(typeof(RemoteStateJsonConverter))]
public enum RemoteState : sbyte
{
    [Display(Name = "ON")]
    On,

    [Display(Name = "OFF")]
    Off,

    [Display(Name = "UNAVAILABLE")]
    Unavailable,

    [Display(Name = "UNKNOWN")]
    Unknown
}
