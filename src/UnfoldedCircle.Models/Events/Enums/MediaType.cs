using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

[EnumJsonConverter<MediaType>(CaseSensitive = false, PropertyName = "media_type")]
[JsonConverter(typeof(MediaTypeJsonConverter))]
public enum MediaType : sbyte
{
    [Display(Name = "MUSIC")]
    Music,
    
    [Display(Name = "RADIO")]
    Radio,
    
    [Display(Name = "TVSHOW")]
    TvShow,
    
    [Display(Name = "MOVIE")]
    Movie,
    
    [Display(Name = "VIDEO")]
    Video
}
