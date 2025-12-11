using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<UserInterfaceItemType>(CaseSensitive = false, PropertyName = "type")]
[JsonConverter(typeof(UserInterfaceItemTypeJsonConverter))]
public enum UserInterfaceItemType : sbyte
{
    [Display(Name = "icon")]
    Icon,

    [Display(Name = "text")]
    Text,

    [Display(Name = "numpad")]
    Numpad
}
