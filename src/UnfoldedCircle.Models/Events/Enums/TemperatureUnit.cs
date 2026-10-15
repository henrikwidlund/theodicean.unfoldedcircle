namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<TemperatureUnit>))]
public enum TemperatureUnit : byte
{
    /// <summary>
    /// Celsius
    /// </summary>
    [JsonStringEnumMemberName("CELSIUS")]
    Celsius,

    /// <summary>
    /// Fahrenheit
    /// </summary>
    [JsonStringEnumMemberName("FAHRENHEIT")]
    Fahrenheit
}
