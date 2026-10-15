namespace UnfoldedCircle.Models.Events;

/// <remarks>
/// The state attribute is optional for a select entity and defaults to ON if not specified.
/// The select entity only supports the <see cref="On"/> state and the common entity states.
/// </remarks>
[JsonConverter(typeof(StrictEnumConverter<SelectState>))]
public enum SelectState : byte
{
    /// <summary>
    /// The selection is available.
    /// </summary>
    [JsonStringEnumMemberName("ON")]
    On,

    /// <summary>
    /// The entity is currently not available. The UI will render the entity as inactive until the entity becomes active again.
    /// </summary>
    [JsonStringEnumMemberName("UNAVAILABLE")]
    Unavailable,

    /// <summary>
    /// The entity is available but the current state is unknown.
    /// </summary>
    [JsonStringEnumMemberName("UNKNOWN")]
    Unknown
}
