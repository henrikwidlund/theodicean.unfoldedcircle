using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<ClimateCommandId>))]
public enum ClimateCommandId : byte
{
    /// <summary>
    /// Switch on the climate device.
    /// </summary>
    [JsonStringEnumMemberName("on")]
    On,

    /// <summary>
    /// Switch off the climate device.
    /// </summary>
    [JsonStringEnumMemberName("off")]
    Off,

    /// <summary>
    /// Set the device to heating, cooling, etc.
    /// </summary>
    /// <see cref="ClimateState"/>
    [JsonStringEnumMemberName("hvac_mode")]
    HvacMode,

    /// <summary>
    /// Change the target temperature
    /// </summary>
    [JsonStringEnumMemberName("target_temperature")]
    TargetTemperature,

    [JsonStringEnumMemberName("target_temperature_range")]
    TargetTemperatureRange,

    [JsonStringEnumMemberName("fan_mode")]
    FanMode
}
