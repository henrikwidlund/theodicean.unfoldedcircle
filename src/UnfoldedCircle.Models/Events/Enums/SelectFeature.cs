using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Events;

/// <summary>
/// The select entity has no features.
/// </summary>
[EnumJsonConverter<SelectFeature>(CaseSensitive = false, PropertyName = "features")]
[JsonConverter(typeof(SelectFeaturesJsonConverter))]
public enum SelectFeature : sbyte;