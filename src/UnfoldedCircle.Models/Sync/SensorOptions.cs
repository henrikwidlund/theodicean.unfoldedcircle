namespace UnfoldedCircle.Models.Sync;

public record SensorOptions
{
    /// <summary>
    /// Unit label for a custom sensor if device_class is not specified or to override a default unit.
    /// </summary>
    [JsonPropertyName("custom_unit")]
    public string? CustomUnit { get; init; }

    /// <summary>
    /// The sensor's native unit of measurement to perform automatic conversion. Applicable to device classes: <see cref="DeviceClass.Temperature"/>.
    /// </summary>
    [JsonPropertyName("native_unit")]
    public string? NativeUnit { get; init; }

    /// <summary>
    /// Number of decimal places to show in the UI if the sensor provides the measurement as a number. Not applicable to string values.
    /// </summary>
    [JsonPropertyName("decimals")]
    public ushort? Decimals { get; init; }

    /// <summary>
    /// Optional minimum value of the sensor output. This can be used in the UI for graphs or gauges.
    /// </summary>
    /// <remarks>Supported unit values are the Home Assistant binary sensor device classes: https://www.home-assistant.io/integrations/binary_sensor/#device-class</remarks>
    [JsonPropertyName("min_value")]
    public int? MinValue { get; init; }

    /// <summary>
    /// Optional maximum value of the sensor output. This can be used in the UI for graphs or gauges.
    /// </summary>
    /// <remarks>Supported unit values are the Home Assistant binary sensor device classes: https://www.home-assistant.io/integrations/binary_sensor/#device-class</remarks>
    [JsonPropertyName("max_value")]
    public int? MaxValue { get; init; }
}