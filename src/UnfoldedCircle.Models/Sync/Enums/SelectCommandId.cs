using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

[EnumJsonConverter<SelectCommandId>(CaseSensitive = false, PropertyName = "cmd_id")]
[JsonConverter(typeof(SelectCommandIdJsonConverter))]
public enum SelectCommandId
{
    /// <summary>
    /// Select a specific option. The option parameter must be one of the values in the options attribute.
    /// </summary>
    [Display(Name = "select_option")]
    SelectOption,

    /// <summary>
    /// Select the first option in the list.
    /// </summary>
    [Display(Name = "select_first")]
    SelectFirst,

    /// <summary>
    /// Select the last option in the list.
    /// </summary>
    [Display(Name = "select_last")]
    SelectLast,

    /// <summary>
    /// Select the next option in the list. If cycle is true, it wraps around to the first option.
    /// </summary>
    [Display(Name = "select_next")]
    SelectNext,

    /// <summary>
    /// Select the previous option in the list. If cycle is true, it wraps around to the last option.
    /// </summary>
    [Display(Name = "select_previous")]
    SelectPrevious
}