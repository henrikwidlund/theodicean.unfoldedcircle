using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

/// <summary>
/// Sensor type. This is used by the UI to represent the sensor with a matching icon, default unit etc.
/// To use a custom unit
/// </summary>
[EnumJsonConverter<DeviceClass>(CaseSensitive = false, PropertyName = "device_class")]
[JsonConverter(typeof(DeviceClassJsonConverter))]
public enum DeviceClass : sbyte
{
    [Display(Name = "custom")]
    Custom = 1,

    [Display(Name = "temperature")]
    Temperature,

    [Display(Name = "humidity")]
    Humidity,

    [Display(Name = "power")]
    Power,

    [Display(Name = "energy")]
    Energy,

    [Display(Name = "voltage")]
    Voltage,

    [Display(Name = "current")]
    Current,

    [Display(Name = "battery")]
    Battery
}