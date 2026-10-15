namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<SelectCommandId>))]
public enum SelectCommandId
{
    /// <summary>
    /// Select a specific option. The option parameter must be one of the values in the options attribute.
    /// </summary>
    [JsonStringEnumMemberName("select_option")]
    SelectOption,

    /// <summary>
    /// Select the first option in the list.
    /// </summary>
    [JsonStringEnumMemberName("select_first")]
    SelectFirst,

    /// <summary>
    /// Select the last option in the list.
    /// </summary>
    [JsonStringEnumMemberName("select_last")]
    SelectLast,

    /// <summary>
    /// Select the next option in the list. If cycle is true, it wraps around to the first option.
    /// </summary>
    [JsonStringEnumMemberName("select_next")]
    SelectNext,

    /// <summary>
    /// Select the previous option in the list. If cycle is true, it wraps around to the last option.
    /// </summary>
    [JsonStringEnumMemberName("select_previous")]
    SelectPrevious
}
