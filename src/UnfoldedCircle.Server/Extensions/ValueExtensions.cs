using System.Collections.Frozen;

using UnfoldedCircle.Models.Shared;

namespace UnfoldedCircle.Server.Extensions;

/// <summary>
/// Extension methods for working with values in dictionaries and identifiers.
/// </summary>
public static class ValueExtensions
{
    internal static bool DisableEntityIdPrefixing { get; set; }

    /// <summary>
    /// Gets a value from the dictionary or returns a default value if the key is not found or its value is null or whitespace.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the value from.</param>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="defaultValue">The value returned if the key is not found, is null, empty or only contains whitespaces.</param>
    /// <typeparam name="TKey"></typeparam>
    // ReSharper disable once UnusedMember.Global
    public static string GetValueOrNull<TKey>(this IReadOnlyDictionary<TKey, string> dictionary, TKey key, string defaultValue)
    {
        var value = dictionary.GetValueOrDefault(key, defaultValue);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// Gets the identifier for a given entity type, ensuring it has the correct prefix.
    /// </summary>
    /// <param name="baseIdentifier">The base value for the identifier.</param>
    /// <param name="entityType">The <see cref="EntityType"/>.</param>
    /// <param name="suffix">
    /// Optional suffix to add to the identifier. Not used for <see cref="EntityType.MediaPlayer"/> and <see cref="EntityType.Remote"/>.
    /// <remarks>Suffixes are always appended with a <c>_</c> plus the suffix value.</remarks>
    /// </param>
    /// <returns>A prefixed identifier based on the <paramref name="entityType"/> value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An unknown value for <see cref="EntityType"/> was specified.</exception>
    public static string GetIdentifier(this string baseIdentifier, in EntityType entityType, string? suffix = null)
    {
        var identifierSpan = baseIdentifier.AsSpan().GetBaseIdentifier();

        if (DisableEntityIdPrefixing)
            return baseIdentifier.Length != identifierSpan.Length ? identifierSpan.ToString() : baseIdentifier;

        var localSuffix = string.IsNullOrWhiteSpace(suffix) ? string.Empty : $"_{suffix}";

        return entityType switch
        {
            EntityType.Cover => GetIdentifierInternal(baseIdentifier, CoverPrefix, localSuffix),
            EntityType.Button => GetIdentifierInternal(baseIdentifier, ButtonPrefix, localSuffix),
            EntityType.Climate => GetIdentifierInternal(baseIdentifier, ClimatePrefix, localSuffix),
            EntityType.Light => GetIdentifierInternal(baseIdentifier, LightPrefix, localSuffix),
            EntityType.MediaPlayer => baseIdentifier.Length != identifierSpan.Length ? identifierSpan.ToString() : baseIdentifier,
            EntityType.Remote => GetIdentifierInternal(baseIdentifier, RemotePrefix, localSuffix),
            EntityType.Select => GetIdentifierInternal(baseIdentifier, SelectPrefix, localSuffix),
            EntityType.Sensor => GetIdentifierInternal(baseIdentifier, SensorPrefix, localSuffix),
            EntityType.Switch => GetIdentifierInternal(baseIdentifier, SwitchPrefix, localSuffix),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, message: null)
        };

        static string GetIdentifierInternal(string identifier, string prefix, string? suffix)
        {
            if (!identifier.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(suffix) && identifier.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return $"{prefix}{identifier}";

                return string.IsNullOrWhiteSpace(suffix)
                    ? $"{prefix}{identifier}"
                    : $"{prefix}{identifier}{suffix}";
            }

            if (string.IsNullOrWhiteSpace(suffix) || identifier.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return identifier;

            return $"{identifier}{suffix}";
        }
    }

    /// <summary>
    /// Gets the identifier for a given entity type, ensuring it has the correct prefix.
    /// </summary>
    /// <param name="baseIdentifier">The base value for the identifier.</param>
    /// <param name="entityType">The <see cref="EntityType"/>.</param>
    /// <returns>A prefixed identifier based on the <paramref name="entityType"/> value.</returns>
    /// <param name="suffix">
    /// Optional suffix to add to the identifier. Not used for <see cref="EntityType.MediaPlayer"/> and <see cref="EntityType.Remote"/>.
    /// <remarks>Suffixes are always appended with a <c>_</c> plus the suffix value.</remarks>
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">An unknown value for <see cref="EntityType"/> was specified.</exception>
    // ReSharper disable once UnusedMember.Global
    public static ReadOnlyMemory<char> GetIdentifier(this ReadOnlyMemory<char> baseIdentifier, in EntityType entityType, string? suffix = null)
    {
        var identifierMemory = baseIdentifier.GetBaseIdentifier();

        if (DisableEntityIdPrefixing)
            return baseIdentifier.Length != identifierMemory.Length ? identifierMemory : baseIdentifier;

        var localSuffix = string.IsNullOrWhiteSpace(suffix) ? string.Empty : $"_{suffix}";

        return entityType switch
        {
            EntityType.Cover => GetIdentifierInternal(baseIdentifier, CoverPrefix, localSuffix),
            EntityType.Button => GetIdentifierInternal(baseIdentifier, ButtonPrefix, localSuffix),
            EntityType.Climate => GetIdentifierInternal(baseIdentifier, ClimatePrefix, localSuffix),
            EntityType.Light => GetIdentifierInternal(baseIdentifier, LightPrefix, localSuffix),
            EntityType.MediaPlayer => baseIdentifier.Length != identifierMemory.Length ? identifierMemory : baseIdentifier,
            EntityType.Remote => GetIdentifierInternal(baseIdentifier, RemotePrefix, localSuffix),
            EntityType.Select => GetIdentifierInternal(baseIdentifier, SelectPrefix, localSuffix),
            EntityType.Sensor => GetIdentifierInternal(baseIdentifier, SensorPrefix, localSuffix),
            EntityType.Switch => GetIdentifierInternal(baseIdentifier, SwitchPrefix, localSuffix),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, message: null)
        };

        static ReadOnlyMemory<char> GetIdentifierInternal(ReadOnlyMemory<char> identifier, string prefix, string? suffix)
        {
            if (!identifier.Span.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(suffix) && identifier.Span.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return $"{prefix}{identifier}".AsMemory();

                return string.IsNullOrWhiteSpace(suffix)
                    ? $"{prefix}{identifier}".AsMemory()
                    : $"{prefix}{identifier}{suffix}".AsMemory();
            }

            if (string.IsNullOrWhiteSpace(suffix) || identifier.Span.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return identifier;

            return $"{identifier}{suffix}".AsMemory();
        }
    }

    private const string CoverPrefix = "COVER:";
    private const string ButtonPrefix = "BUTTON:";
    private const string ClimatePrefix = "CLIMATE:";
    private const string LightPrefix = "LIGHT:";
    private const string RemotePrefix = "REMOTE:";
    private const string SelectPrefix = "SELECT:";
    private const string SensorPrefix = "SENSOR:";
    private const string SwitchPrefix = "SWITCH:";

    private static readonly FrozenSet<string> PrefixesSet =
    [
        CoverPrefix, ButtonPrefix, ClimatePrefix, LightPrefix, RemotePrefix, SelectPrefix, SensorPrefix, SwitchPrefix
    ];

    /// <summary>
    /// Gets the identifier for a given entity type, ensuring it has the correct prefix.
    /// </summary>
    /// <param name="baseIdentifier">The base value for the identifier.</param>
    /// <param name="entityType">The <see cref="EntityType"/>.</param>
    /// <param name="suffix">
    /// Optional suffix to add to the identifier. Not used for <see cref="EntityType.MediaPlayer"/> and <see cref="EntityType.Remote"/>.
    /// <remarks>Suffixes are always appended with a <c>_</c> plus the suffix value.</remarks>
    /// </param>
    /// <returns>
    /// A prefixed identifier based on the <paramref name="entityType"/> value,
    /// or null if the <paramref name="baseIdentifier"/> is null or whitespace.
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string? GetNullableIdentifier(this string? baseIdentifier, in EntityType entityType, string? suffix = null)
        => string.IsNullOrWhiteSpace(baseIdentifier) ? null : baseIdentifier.GetIdentifier(entityType, suffix);

    /// <summary>
    /// Gets the base identifier for the <paramref name="identifier"/> by stripping away any prefix.
    /// </summary>
    /// <param name="identifier">The identifier to get the base identifier for.</param>
    /// <returns>The base identifier.</returns>
    public static string GetBaseIdentifier(this string identifier) => identifier.GetIdentifier(EntityType.MediaPlayer);

    /// <summary>
    /// Gets the base identifier for the <paramref name="identifier"/> by stripping away any prefix.
    /// </summary>
    /// <param name="identifier">The identifier to get the base identifier for.</param>
    /// <returns>The base identifier.</returns>
    public static ReadOnlyMemory<char> GetBaseIdentifier(this in ReadOnlyMemory<char> identifier)
    {
        var identifierMemory = identifier;
        var prefix = PrefixesSet.FirstOrDefault(p => identifierMemory.Span.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (prefix is not null)
        {
            identifierMemory = identifierMemory[prefix.Length..];
            if (!prefix.Equals(RemotePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var underscoreIndex = identifierMemory.Span.LastIndexOf('_');
                if (underscoreIndex >= 0)
                    identifierMemory = identifierMemory[..underscoreIndex];
            }
        }

        return identifierMemory;
    }

    /// <summary>
    /// Gets the base identifier for the <paramref name="identifier"/> by stripping away any prefix.
    /// </summary>
    /// <param name="identifier">The identifier to get the base identifier for.</param>
    /// <returns>The base identifier.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static ReadOnlySpan<char> GetBaseIdentifier(this in ReadOnlySpan<char> identifier)
    {
        var identifierSpan = identifier;
        foreach (string se in PrefixesSet)
        {
            if (identifier.StartsWith(se, StringComparison.OrdinalIgnoreCase))
            {
                identifierSpan = identifierSpan[se.Length..];
                if (!se.Equals(RemotePrefix, StringComparison.Ordinal))
                {
                    var underscoreIndex = identifierSpan.LastIndexOf('_');
                    if (underscoreIndex >= 0)
                        identifierSpan = identifierSpan[..underscoreIndex];
                }
                break;
            }
        }

        return identifierSpan;
    }

    /// <summary>
    /// Gets the base identifier for the <paramref name="identifier"/> by stripping away any prefix.
    /// </summary>
    /// <param name="identifier">The identifier to get the base identifier for.</param>
    /// <returns>The base identifier, or null if <paramref name="identifier"/> is null or whitespace.</returns>
    public static string? GetNullableBaseIdentifier(this string? identifier) => identifier.GetNullableIdentifier(EntityType.MediaPlayer);

    /// <summary>
    /// Gets the base identifier for the <paramref name="identifier"/> by stripping away any prefix.
    /// </summary>
    /// <param name="identifier">The identifier to get the base identifier for.</param>
    /// <returns>The base identifier, or null if <paramref name="identifier"/> is null or whitespace.</returns>
    // ReSharper disable once UnusedMember.Global
    public static ReadOnlyMemory<char>? GetNullableBaseIdentifier(this in ReadOnlyMemory<char>? identifier)
        => identifier == null || identifier.Value.IsEmpty ? null : identifier.Value.GetBaseIdentifier();

    /// <summary>
    /// Gets the <see cref="EntityType"/> from the given <paramref name="identifier"/>.
    /// </summary>
    /// <remarks>
    /// If the identifier is empty or does not start with any known prefix, <see cref="EntityType.MediaPlayer"/> is returned.
    /// </remarks>
    /// <param name="identifier">
    /// The identifier to get the type from. If it does not start with any of the known entity prefixes,
    /// it will be treated as a media player identifier and <see cref="EntityType.MediaPlayer"/> will be returned.
    /// </param>
    /// <returns>
    /// The resolved <see cref="EntityType"/> based on the identifier prefix, or <see cref="EntityType.MediaPlayer"/>
    /// when the identifier is empty or its prefix is unrecognized.
    /// </returns>
    public static EntityType GetEntityTypeFromIdentifier(this in ReadOnlySpan<char> identifier) =>
        identifier switch
        {
            _ when identifier.StartsWith(CoverPrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Cover,
            _ when identifier.StartsWith(ButtonPrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Button,
            _ when identifier.StartsWith(ClimatePrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Climate,
            _ when identifier.StartsWith(LightPrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Light,
            _ when identifier.StartsWith(RemotePrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Remote,
            _ when identifier.StartsWith(SelectPrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Select,
            _ when identifier.StartsWith(SensorPrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Sensor,
            _ when identifier.StartsWith(SwitchPrefix, StringComparison.OrdinalIgnoreCase) => EntityType.Switch,
            _ => EntityType.MediaPlayer
        };

    /// <summary>
    /// Gets the suffix from the given <paramref name="identifier"/> if it ends with a suffix, or <see langword="null"/> if it does not have a suffix.
    /// </summary>
    /// <param name="identifier">The identifier to get the suffix from.</param>
    /// <returns>The suffix of the identifier, or <see langword="null"/> if it does not have a suffix.</returns>
    public static string? GetSuffix(this in ReadOnlySpan<char> identifier)
    {
        var suffixStartIndex = identifier.LastIndexOf('_');
        return suffixStartIndex < 0 ? null : identifier[(suffixStartIndex + 1)..].ToString();
    }
}