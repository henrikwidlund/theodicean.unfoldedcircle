using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

/// <summary>
/// Integration driver authentication method if a token is required.
///
/// The JSON `auth` message is used if a token is configured but no authentication method is set.
/// </summary>
[EnumJsonConverter<IntgAuthMethod>(CaseSensitive = false, PropertyName = "auth_method")]
[JsonConverter(typeof(IntgAuthMethodJsonConverter))]
public enum IntgAuthMethod : sbyte
{
    [Display(Name = "HEADER")]
    Header = 1,
    
    [Display(Name = "MESSAGE")]
    Message
}
