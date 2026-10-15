namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<RemoteEntityAttribute>))]
public enum RemoteEntityAttribute : byte
{
    /// <summary>
    /// State of the controlled device, it's either on or off.
    /// </summary>
    [JsonStringEnumMemberName("state")]
    State = 1
}
