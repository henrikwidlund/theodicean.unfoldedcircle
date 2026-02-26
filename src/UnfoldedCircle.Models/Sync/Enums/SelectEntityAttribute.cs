using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

[EnumJsonConverter<SelectEntityAttribute>(CaseSensitive = false, PropertyName = "attributes")]
[JsonConverter(typeof(SelectEntityAttributeJsonConverter))]
public enum SelectEntityAttribute : sbyte
{
    /// <summary>
    /// Optional state of the select entity.
    /// </summary>
    [Display(Name = "state")]
    State = 1,

    /// <summary>
    /// The currently selected option.
    /// </summary>
    [Display(Name = "current_option")]
    CurrentOption,

    /// <summary>
    /// The available options to choose from.
    /// </summary>
    [Display(Name = "options")]
    Options
}