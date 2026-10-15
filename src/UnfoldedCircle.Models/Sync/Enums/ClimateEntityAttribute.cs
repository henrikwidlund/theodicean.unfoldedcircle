using UnfoldedCircle.Models.Events;

namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<ClimateEntityAttribute>))]
public enum ClimateEntityAttribute : byte
{
    /// <summary>
    /// State of the climate device, corresponds to <see cref="HvacMode"/>.
    /// </summary>
    [JsonStringEnumMemberName("state")]
    State = 1,

    [JsonStringEnumMemberName("current_temperature")]
    CurrentTemperature,

    [JsonStringEnumMemberName("target_temperature")]
    TargetTemperature
}
