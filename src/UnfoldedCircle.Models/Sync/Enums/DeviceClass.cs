namespace UnfoldedCircle.Models.Sync;

/// <summary>
/// Sensor type. This is used by the UI to represent the sensor with a matching icon, default unit etc.
/// To use a custom unit
/// </summary>
[JsonConverter(typeof(StrictEnumConverter<DeviceClass>))]
public enum DeviceClass : byte
{
    [JsonStringEnumMemberName("custom")]
    Custom = 1,

    [JsonStringEnumMemberName("temperature")]
    Temperature,

    [JsonStringEnumMemberName("humidity")]
    Humidity,

    [JsonStringEnumMemberName("power")]
    Power,

    [JsonStringEnumMemberName("energy")]
    Energy,

    [JsonStringEnumMemberName("voltage")]
    Voltage,

    [JsonStringEnumMemberName("current")]
    Current,

    [JsonStringEnumMemberName("battery")]
    Battery
}
