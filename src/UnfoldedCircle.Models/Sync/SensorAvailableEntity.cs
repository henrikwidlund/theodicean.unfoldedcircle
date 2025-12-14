namespace UnfoldedCircle.Models.Sync;

/// <summary>
/// A sensor entity provides measured values from devices or dedicated hardware sensors.
/// The device class specifies the type of sensor and links it with a default unit of
/// measurement to display in the user interface.
///
/// <list type="bullet">
/// <item>The <see cref="Sync.DeviceClass.Custom"/> device class allows arbitrary UI labels and units.</item>
/// <item>The <see cref="Sync.DeviceClass.Temperature"/> device class performs automatic conversion between °C and °F.</item>
/// </list>
///
/// See (https://github.com/unfoldedcircle/core-api/blob/main/doc/entities/entity_sensor.md
/// for more information.
/// </summary>
public record SensorAvailableEntity : AvailableEntity<SensorFeature, SensorOptions>
{
    [JsonPropertyName("device_class")]
    public required DeviceClass DeviceClass { get; init; }
}