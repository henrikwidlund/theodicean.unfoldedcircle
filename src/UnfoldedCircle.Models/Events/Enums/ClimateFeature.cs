namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<ClimateFeature>))]
public enum ClimateFeature : byte
{
    /// <summary>
    /// The device can be turned on and off.
    /// The active HVAC mode after power on is device specific and must be reflected in the state attribute.
    /// </summary>
    [JsonStringEnumMemberName("on_off")]
    OnOff,

    /// <summary>
    /// The device supports heating.
    /// </summary>
    [JsonStringEnumMemberName("heat")]
    Heat,

    /// <summary>
    /// The device supports cooling.
    /// </summary>
    [JsonStringEnumMemberName("cool")]
    Cool,

    /// <summary>
    /// The device can measure the current temperature.
    /// </summary>
    [JsonStringEnumMemberName("current_temperature")]
    CurrentTemperature,

    /// <summary>
    /// The device supports a target temperature for heating or cooling.
    /// </summary>
    [JsonStringEnumMemberName("target_temperature")]
    TargetTemperature,

    /// <summary>
    /// The device supports a target temperature range.
    /// </summary>
    [JsonStringEnumMemberName("target_temperature_range")]
    TargetTemperatureRange,

    /// <summary>
    /// The device has a controllable fan.
    /// </summary>
    [JsonStringEnumMemberName("fan")]
    Fan
}
