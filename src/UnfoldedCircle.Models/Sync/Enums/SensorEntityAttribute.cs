namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<SensorEntityAttribute>))]
public enum SensorEntityAttribute : byte
{
    /// <summary>
    /// Optional state of the sensor.
    /// </summary>
    [JsonStringEnumMemberName("state")]
    State = 1,

    /// <summary>
    /// The native measurement value of the sensor.
    /// </summary>
    [JsonStringEnumMemberName("value")]
    Value,

    /// <summary>
    /// Optional unit of the value if no default unit is set.
    /// </summary>
    [JsonStringEnumMemberName("unit")]
    Unit
}
