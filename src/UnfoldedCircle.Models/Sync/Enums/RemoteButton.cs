namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<RemoteButton>))]
public enum RemoteButton : byte
{
    [JsonStringEnumMemberName(RemoteButtonConstants.On)]
    On = 1,

    [JsonStringEnumMemberName(RemoteButtonConstants.Off)]
    Off,

    [JsonStringEnumMemberName(RemoteButtonConstants.Toggle)]
    Toggle,

    [JsonStringEnumMemberName(RemoteButtonConstants.Home)]
    Home,

    [JsonStringEnumMemberName(RemoteButtonConstants.Back)]
    Back,

    [JsonStringEnumMemberName(RemoteButtonConstants.Voice)]
    Voice,

    [JsonStringEnumMemberName(RemoteButtonConstants.DpadUp)]
    DpadUp,

    [JsonStringEnumMemberName(RemoteButtonConstants.DpadDown)]
    DpadDown,

    [JsonStringEnumMemberName(RemoteButtonConstants.DpadLeft)]
    DpadLeft,

    [JsonStringEnumMemberName(RemoteButtonConstants.DpadRight)]
    DpadRight,

    [JsonStringEnumMemberName(RemoteButtonConstants.DpadMiddle)]
    DpadMiddle,

    [JsonStringEnumMemberName(RemoteButtonConstants.ChannelUp)]
    ChannelUp,

    [JsonStringEnumMemberName(RemoteButtonConstants.ChannelDown)]
    ChannelDown,

    [JsonStringEnumMemberName(RemoteButtonConstants.VolumeUp)]
    VolumeUp,

    [JsonStringEnumMemberName(RemoteButtonConstants.VolumeDown)]
    VolumeDown,

    [JsonStringEnumMemberName(RemoteButtonConstants.Power)]
    Power,

    [JsonStringEnumMemberName(RemoteButtonConstants.Mute)]
    Mute,

    [JsonStringEnumMemberName(RemoteButtonConstants.Green)]
    Green,

    [JsonStringEnumMemberName(RemoteButtonConstants.Yellow)]
    Yellow,

    [JsonStringEnumMemberName(RemoteButtonConstants.Red)]
    Red,

    [JsonStringEnumMemberName(RemoteButtonConstants.Blue)]
    Blue,

    [JsonStringEnumMemberName(RemoteButtonConstants.Previous)]
    Previous,

    [JsonStringEnumMemberName(RemoteButtonConstants.Play)]
    Play,

    [JsonStringEnumMemberName(RemoteButtonConstants.Next)]
    Next,

    [JsonStringEnumMemberName(RemoteButtonConstants.Stop)]
    Stop,

    [JsonStringEnumMemberName(RemoteButtonConstants.Record)]
    Record,

    [JsonStringEnumMemberName(RemoteButtonConstants.Menu)]
    Menu
}
