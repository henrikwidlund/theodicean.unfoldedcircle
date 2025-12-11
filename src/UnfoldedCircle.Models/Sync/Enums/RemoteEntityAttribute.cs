using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

[EnumJsonConverter<RemoteEntityAttribute>(CaseSensitive = false, PropertyName = "attributes")]
[JsonConverter(typeof(RemoteEntityAttributeJsonConverter))]
public enum RemoteEntityAttribute : sbyte
{
    /// <summary>
    /// State of the controlled device, it's either on or off.
    /// </summary>
    [Display(Name = "state")]
    State = 1
}
