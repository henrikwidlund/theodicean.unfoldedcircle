using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<RemoteFeature>(CaseSensitive = false, PropertyName = "features")]
[JsonConverter(typeof(RemoteFeaturesJsonConverter))]
public enum RemoteFeature : sbyte
{
    [Display(Name = "send_cmd")]
    SendCmd,

    [Display(Name = "on_off")]
    OnOff,

    [Display(Name = "toggle")]
    Toggle
}
