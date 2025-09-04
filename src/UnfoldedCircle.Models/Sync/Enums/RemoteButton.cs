using System.ComponentModel.DataAnnotations;

using Theodicean.SourceGenerators;

namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(RemoteCommandIdJsonConverter))]
public enum RemoteButton
{
    [Display(Name = RemoteButtonConstants.On)]
    On = 1,

    [Display(Name = RemoteButtonConstants.Off)]
    Off,

    [Display(Name = RemoteButtonConstants.Toggle)]
    Toggle,

    [Display(Name = RemoteButtonConstants.Home)]
    Home,

    [Display(Name = RemoteButtonConstants.Back)]
    Back,

    [Display(Name = RemoteButtonConstants.Voice)]
    Voice,

    [Display(Name = RemoteButtonConstants.DpadUp)]
    DpadUp,

    [Display(Name = RemoteButtonConstants.DpadDown)]
    DpadDown,

    [Display(Name = RemoteButtonConstants.DpadLeft)]
    DpadLeft,

    [Display(Name = RemoteButtonConstants.DpadRight)]
    DpadRight,

    [Display(Name = RemoteButtonConstants.DpadMiddle)]
    DpadMiddle,

    [Display(Name = RemoteButtonConstants.ChannelUp)]
    ChannelUp,

    [Display(Name = RemoteButtonConstants.ChannelDown)]
    ChannelDown,

    [Display(Name = RemoteButtonConstants.VolumeUp)]
    VolumeUp,

    [Display(Name = RemoteButtonConstants.VolumeDown)]
    VolumeDown,

    [Display(Name = RemoteButtonConstants.Power)]
    Power,

    [Display(Name = RemoteButtonConstants.Mute)]
    Mute,

    [Display(Name = RemoteButtonConstants.Green)]
    Green,

    [Display(Name = RemoteButtonConstants.Yellow)]
    Yellow,

    [Display(Name = RemoteButtonConstants.Red)]
    Red,

    [Display(Name = RemoteButtonConstants.Blue)]
    Blue,

    [Display(Name = RemoteButtonConstants.Previous)]
    Previous,

    [Display(Name = RemoteButtonConstants.Play)]
    Play,

    [Display(Name = RemoteButtonConstants.Next)]
    Next,

    [Display(Name = RemoteButtonConstants.Stop)]
    Stop,

    [Display(Name = RemoteButtonConstants.Record)]
    Record,

    [Display(Name = RemoteButtonConstants.Menu)]
    Menu
}

[EnumJsonConverter(typeof(RemoteButton), CaseSensitive = false, PropertyName = "button")]
public partial class RemoteCommandIdJsonConverter;