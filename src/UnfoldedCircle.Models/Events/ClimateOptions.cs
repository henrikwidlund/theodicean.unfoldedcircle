using System.ComponentModel.DataAnnotations;

namespace UnfoldedCircle.Models.Events;

public record ClimateOptions
{
    /// <summary>
    /// The unit of temperature measurement. If not specified, the remote settings are used.
    /// </summary>
    [JsonPropertyName("temperature_unit")]
    public TemperatureUnit? TemperatureUnit { get; init; }

    /// <summary>
    /// Step value for the UI for setting the target temperature.
    /// Defaults: <see cref="TemperatureUnit.Celsius" /> = 0.5, <see cref="TemperatureUnit.Fahrenheit" /> = 1.
    /// Smallest step size: 0.1
    /// </summary>
    [Range(minimum: 0.1, maximum: float.MaxValue, ErrorMessage = "The step value must be at least 0.1.")]
    [JsonPropertyName("target_temperature_step")]
    public float? TargetTemperatureStep { get; init; }

    /// <summary>
    /// Maximum temperature to show in the UI for the target temperature range.
    /// </summary>
    /// <remarks>Default: 30</remarks>
    [JsonPropertyName("max_temperature")]
    public float? MaxTemperature { get; init; }

    /// <summary>
    /// Minimum temperature to show in the UI for the target temperature range.
    /// </summary>
    /// <remarks>Default: 10</remarks>
    [JsonPropertyName("min_temperature")]
    public float? MinTemperature { get; set; }
}