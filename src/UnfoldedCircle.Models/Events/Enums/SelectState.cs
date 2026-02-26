using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

/// <remarks>
/// The state attribute is optional for a select entity and defaults to ON if not specified.
/// The select entity only supports the <see cref="On"/> state and the common entity states.
/// </remarks>
[EnumJsonConverter<SelectState>(CaseSensitive = false, PropertyName = "state")]
[JsonConverter(typeof(SelectStateJsonConverter))]
public enum SelectState
{
    /// <summary>
    /// The selection is available.
    /// </summary>
    [Display(Name = "ON")]
    On,

    /// <summary>
    /// The entity is currently not available. The UI will render the entity as inactive until the entity becomes active again.
    /// </summary>
    [Display(Name = "UNAVAILABLE")]
    Unavailable,

    /// <summary>
    /// The entity is available but the current state is unknown.
    /// </summary>
    [Display(Name = "UNKNOWN")]
    Unknown
}