namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<MediaPlayerEntityFeature>))]
public enum MediaPlayerEntityFeature : byte
{
    [JsonStringEnumMemberName("on_off")]
    OnOff = 1,

    [JsonStringEnumMemberName("toggle")]
    Toggle,

    [JsonStringEnumMemberName("volume")]
    Volume,

    [JsonStringEnumMemberName("volume_up_down")]
    VolumeUpDown,

    [JsonStringEnumMemberName("mute_toggle")]
    MuteToggle,

    [JsonStringEnumMemberName("mute")]
    Mute,

    [JsonStringEnumMemberName("unmute")]
    Unmute,

    [JsonStringEnumMemberName("play_pause")]
    PlayPause,

    [JsonStringEnumMemberName("stop")]
    Stop,

    [JsonStringEnumMemberName("next")]
    Next,

    [JsonStringEnumMemberName("previous")]
    Previous,

    [JsonStringEnumMemberName("fast_forward")]
    FastForward,

    [JsonStringEnumMemberName("rewind")]
    Rewind,

    [JsonStringEnumMemberName("repeat")]
    Repeat,

    [JsonStringEnumMemberName("shuffle")]
    Shuffle,

    [JsonStringEnumMemberName("seek")]
    Seek,

    [JsonStringEnumMemberName("media_duration")]
    MediaDuration,

    [JsonStringEnumMemberName("media_position")]
    MediaPosition,

    [JsonStringEnumMemberName("media_title")]
    MediaTitle,

    [JsonStringEnumMemberName("media_artist")]
    MediaArtist,

    [JsonStringEnumMemberName("media_album")]
    MediaAlbum,

    [JsonStringEnumMemberName("media_image_url")]
    MediaImageUrl,

    [JsonStringEnumMemberName("media_type")]
    MediaType,

    [JsonStringEnumMemberName("dpad")]
    Dpad,

    [JsonStringEnumMemberName("numpad")]
    Numpad,

    [JsonStringEnumMemberName("home")]
    Home,

    [JsonStringEnumMemberName("menu")]
    Menu,

    [JsonStringEnumMemberName("context_menu")]
    ContextMenu,

    [JsonStringEnumMemberName("guide")]
    Guide,

    [JsonStringEnumMemberName("info")]
    Info,

    [JsonStringEnumMemberName("color_buttons")]
    ColorButtons,

    [JsonStringEnumMemberName("channel_switcher")]
    ChannelSwitcher,

    [JsonStringEnumMemberName("select_source")]
    SelectSource,

    [JsonStringEnumMemberName("select_sound_mode")]
    SelectSoundMode,

    [JsonStringEnumMemberName("eject")]
    Eject,

    [JsonStringEnumMemberName("open_close")]
    OpenClose,

    [JsonStringEnumMemberName("audio_track")]
    AudioTrack,

    [JsonStringEnumMemberName("subtitle")]
    Subtitle,

    [JsonStringEnumMemberName("record")]
    Record,

    [JsonStringEnumMemberName("settings")]
    Settings
}
