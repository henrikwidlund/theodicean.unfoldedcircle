namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<SensorFeature>))]
public enum SensorFeature : byte;
