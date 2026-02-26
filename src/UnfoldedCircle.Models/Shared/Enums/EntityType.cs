using System.ComponentModel.DataAnnotations;
using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Shared;

[EnumJsonConverter<EntityType>(CaseSensitive = false, PropertyName = "entity_type")]
[JsonConverter(typeof(EntityTypeJsonConverter))]
public enum EntityType : sbyte
{
    [Display(Name = "button")]
    Button = 1,
    
    [Display(Name = "climate")]
    Climate,

    [Display(Name = "cover")]
    Cover,

    [Display(Name = "light")]
    Light,
    
    [Display(Name = "media_player")]
    MediaPlayer,

    [Display(Name = "remote")]
    Remote,

    [Display(Name = "select")]
    Select,

    [Display(Name = "sensor")]
    Sensor,
    
    [Display(Name = "switch")]
    Switch
}
