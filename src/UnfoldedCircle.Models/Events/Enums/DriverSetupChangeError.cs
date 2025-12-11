using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<DriverSetupChangeError>(CaseSensitive = false, PropertyName = "error")]
[JsonConverter(typeof(DriverSetupChangeErrorJsonConverter))]
public enum DriverSetupChangeError : sbyte
{
    [Display(Name = "NONE")]
    None = 1,
    
    [Display(Name = "NOT_FOUND")]
    NotFound,
    
    [Display(Name = "CONNECTION_REFUSED")]
    ConnectionRefused,
    
    [Display(Name = "AUTHORIZATION_ERROR")]
    AuthorizationError,
    
    [Display(Name = "TIMEOUT")]
    Timeout,
    
    [Display(Name = "OTHER")]
    Other
}
