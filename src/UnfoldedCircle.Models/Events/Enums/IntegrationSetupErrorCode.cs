using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<IntegrationSetupErrorCode>(CaseSensitive = false, PropertyName = "error")]
[JsonConverter(typeof(IntegrationSetupErrorCodeJsonConverter))]
public enum IntegrationSetupErrorCode : sbyte
{
    [Display(Name = "NONE")]
    None = 1,
    
    [Display(Name = "NOT_FOUND")]
    NotFound,
    
    [Display(Name = "CONFIGURATION_REFUSED")]
    ConfigurationRefused,
    
    [Display(Name = "AUTHORIZATION_ERROR")]
    AuthorizationError,
    
    [Display(Name = "TIMEOUT")]
    Timeout,
    
    [Display(Name = "OTHER")]
    Other
}
