namespace UnfoldedCircle.Models.Events;

/// <summary>
/// The select entity has no features.
/// </summary>
[JsonConverter(typeof(StrictEnumConverter<SelectFeature>))]
public enum SelectFeature : byte;
