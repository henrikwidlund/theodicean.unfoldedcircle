namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<SelectEntityAttribute>))]
public enum SelectEntityAttribute : byte
{
    /// <summary>
    /// Optional state of the select entity.
    /// </summary>
    [JsonStringEnumMemberName("state")]
    State = 1,

    /// <summary>
    /// The currently selected option.
    /// </summary>
    [JsonStringEnumMemberName("current_option")]
    CurrentOption,

    /// <summary>
    /// The available options to choose from.
    /// </summary>
    [JsonStringEnumMemberName("options")]
    Options
}
