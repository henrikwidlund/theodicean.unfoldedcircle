using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Shared;

[EnumJsonConverter<Kind>(CaseSensitive = false, PropertyName = "kind")]
[JsonConverter(typeof(KindJsonConverter))]
public enum Kind : sbyte
{
    [Display(Name = "req")]
    Request = 1,
    
    [Display(Name = "resp")]
    Response,
    
    [Display(Name = "event")]
    Event
}
