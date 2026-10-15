namespace UnfoldedCircle.Models.Events;

[JsonConverter(typeof(StrictEnumConverter<UserInterfaceItemType>))]
public enum UserInterfaceItemType : byte
{
    [JsonStringEnumMemberName("icon")]
    Icon,

    [JsonStringEnumMemberName("text")]
    Text,

    [JsonStringEnumMemberName("numpad")]
    Numpad
}
