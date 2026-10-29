using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Sync;

[JsonDerivedType(typeof(ClimateEntityStateChanged))]
[JsonDerivedType(typeof(MediaPlayerEntityStateChanged))]
[JsonDerivedType(typeof(RemoteEntityStateChanged))]
[JsonDerivedType(typeof(SelectEntityStateChanged))]
[JsonDerivedType(typeof(SensorEntityStateChanged<sbyte>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<byte>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<short>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<ushort>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<int>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<uint>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<nint>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<long>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<ulong>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<decimal>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<double>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<float>))]
[JsonDerivedType(typeof(SensorEntityStateChanged<string>))]
public abstract record EntityStateChanged
{
    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("entity_type")]
    public required EntityType EntityType { get; init; }

    [JsonPropertyName("entity_id")]
    public required string EntityId { get; init; }
}

/// <summary>
/// Current entity attributes, same payload as in the <c>entity_change</c> event.
/// </summary>
public abstract record EntityStateChanged<TAttributes> : EntityStateChanged
    where TAttributes : StateChangedEventMessageDataAttributes
{
    [JsonPropertyName("attributes")]
    public required TAttributes Attributes { get; init; }
}
