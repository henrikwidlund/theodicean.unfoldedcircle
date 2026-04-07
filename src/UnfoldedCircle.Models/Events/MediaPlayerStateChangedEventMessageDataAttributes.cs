using System.ComponentModel.DataAnnotations;

using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Models.Events;

public abstract record MediaPlayerStateChangedEventMessageDataAttributesBase : StateChangedEventMessageDataAttributes
{
    public abstract State? State { get; init; }
    public abstract ushort? Volume { get; init; }
    public abstract bool? Muted { get; init; }
    public abstract uint? MediaPosition { get; init; }
    public abstract uint? MediaDuration { get; init; }
    public abstract string? MediaTitle { get; init; }
    public abstract string? MediaArtist { get; init; }
    public abstract string? MediaAlbum { get; init; }
    public abstract Uri? MediaImageUrl { get; init; }
    public abstract MediaType? MediaType { get; init; }
    public abstract RepeatMode? Repeat { get; init; }
    public abstract bool? Shuffle { get; init; }
    public abstract string? Source { get; init; }
    public abstract string[]? SourceList { get; init; }
    public abstract string? SoundMode { get; init; }
    public abstract string[]? SoundModeList { get; init; }
}

public sealed record DeltaMediaPlayerStateChangedEventMessageDataAttributes : MediaPlayerStateChangedEventMessageDataAttributesBase
{
    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override State? State { get; init; }
    
    [JsonPropertyName("volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Range(0, 100)]
    public override ushort? Volume { get; init; }
    
    [JsonPropertyName("muted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override bool? Muted { get; init; }
    
    [JsonPropertyName("media_position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override uint? MediaPosition { get; init; }
    
    [JsonPropertyName("media_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override uint? MediaDuration { get; init; }
    
    [JsonPropertyName("media_title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override string? MediaTitle { get; init; }
    
    [JsonPropertyName("media_artist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override string? MediaArtist { get; init; }
    
    [JsonPropertyName("media_album")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override string? MediaAlbum { get; init; }
    
    [JsonPropertyName("media_image_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override Uri? MediaImageUrl { get; init; }
    
    [JsonPropertyName("media_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override MediaType? MediaType { get; init; }

    [JsonPropertyName("repeat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override RepeatMode? Repeat { get; init; }
    
    [JsonPropertyName("shuffle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override bool? Shuffle { get; init; }
    
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override string? Source { get; init; }
    
    [JsonPropertyName("source_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string[]? SourceList { get; init; }
    
    [JsonPropertyName("sound_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public override string? SoundMode { get; init; }
    
    [JsonPropertyName("sound_mode_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string[]? SoundModeList { get; init; }
}

public sealed record MediaPlayerStateChangedEventMessageDataAttributes : MediaPlayerStateChangedEventMessageDataAttributesBase
{
    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override State? State { get; init; }

    [JsonPropertyName("volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Range(0, 100)]
    public override ushort? Volume { get; init; }

    [JsonPropertyName("muted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override bool? Muted { get; init; }

    [JsonPropertyName("media_position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override uint? MediaPosition { get; init; }

    [JsonPropertyName("media_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override uint? MediaDuration { get; init; }

    [JsonPropertyName("media_title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string? MediaTitle { get; init; }

    [JsonPropertyName("media_artist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string? MediaArtist { get; init; }

    [JsonPropertyName("media_album")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string? MediaAlbum { get; init; }

    [JsonPropertyName("media_image_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override Uri? MediaImageUrl { get; init; }

    [JsonPropertyName("media_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override MediaType? MediaType { get; init; }

    [JsonPropertyName("repeat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override RepeatMode? Repeat { get; init; }

    [JsonPropertyName("shuffle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override bool? Shuffle { get; init; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string? Source { get; init; }

    [JsonPropertyName("source_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string[]? SourceList { get; init; }

    [JsonPropertyName("sound_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string? SoundMode { get; init; }

    [JsonPropertyName("sound_mode_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override string[]? SoundModeList { get; init; }
}