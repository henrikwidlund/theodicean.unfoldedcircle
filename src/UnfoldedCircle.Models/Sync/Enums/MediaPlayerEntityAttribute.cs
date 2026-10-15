namespace UnfoldedCircle.Models.Sync;

[JsonConverter(typeof(StrictEnumConverter<MediaPlayerEntityAttribute>))]
public enum MediaPlayerEntityAttribute : byte
{
    /// <summary>
    /// State of the media player, influenced by the play and power commands.
    /// </summary>
    [JsonStringEnumMemberName("state")]
    State = 1,

    /// <summary>
    /// Current volume level.
    /// </summary>
    [JsonStringEnumMemberName("volume")]
    Volume,

    /// <summary>
    /// Flag if the volume is muted.
    /// </summary>
    [JsonStringEnumMemberName("muted")]
    Muted,

    /// <summary>
    /// Media duration in seconds.
    /// </summary>
    [JsonStringEnumMemberName("media_duration")]
    MediaDuration,

    /// <summary>
    /// Current media position in seconds.
    /// </summary>
    [JsonStringEnumMemberName("media_position")]
    MediaPosition,

    /// <summary>
    /// The type of media being played.
    /// </summary>
    [JsonStringEnumMemberName("media_type")]
    MediaType,

    /// <summary>
    /// URL to retrieve the album art or an image representing what's being played.
    /// </summary>
    [JsonStringEnumMemberName("media_image_url")]
    MediaImageUrl,

    /// <summary>
    /// Currently playing media information.
    /// </summary>
    [JsonStringEnumMemberName("media_title")]
    MediaTitle,

    /// <summary>
    /// Currently playing media information.
    /// </summary>
    [JsonStringEnumMemberName("media_artist")]
    MediaArtist,

    /// <summary>
    /// Currently playing media information.
    /// </summary>
    [JsonStringEnumMemberName("media_album")]
    MediaAlbum,

    /// <summary>
    /// Current repeat mode.
    /// </summary>
    [JsonStringEnumMemberName("repeat")]
    Repeat,

    /// <summary>
    /// Shuffle mode on or off.
    /// </summary>
    [JsonStringEnumMemberName("shuffle")]
    Shuffle,

    /// <summary>
    /// Currently selected media or input source.
    /// </summary>
    [JsonStringEnumMemberName("source")]
    Source,

    /// <summary>
    /// Available media or input sources.
    /// </summary>
    [JsonStringEnumMemberName("source_list")]
    SourceList,

    /// <summary>
    /// Currently selected sound mode.
    /// </summary>
    [JsonStringEnumMemberName("sound_mode")]
    SoundMode,

    /// <summary>
    /// Available sound modes.
    /// </summary>
    [JsonStringEnumMemberName("sound_mode_list")]
    SoundModeList
}
