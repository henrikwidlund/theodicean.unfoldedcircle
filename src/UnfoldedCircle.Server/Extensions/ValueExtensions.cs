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
        var identifierSpan = GetBaseIdentifier(baseIdentifier.AsSpan());

        if (DisableEntityIdPrefixing)
            return baseIdentifier.Length != identifierSpan.Length ? identifierSpan.ToString() : baseIdentifier;

        var localSuffix = string.IsNullOrWhiteSpace(suffix) ? string.Empty : $"_{suffix}";

        return entityType switch
        {
            EntityType.Cover => baseIdentifier.StartsWith(CoverPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"COVER:{identifierSpan}{localSuffix}",
            EntityType.Button => baseIdentifier.StartsWith(ButtonPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"BUTTON:{identifierSpan}{localSuffix}",
            EntityType.Climate => baseIdentifier.StartsWith(ClimatePrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"CLIMATE:{identifierSpan}{localSuffix}",
            EntityType.Light => baseIdentifier.StartsWith(LightPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"LIGHT:{identifierSpan}",
            EntityType.MediaPlayer => baseIdentifier.Length != identifierSpan.Length ? identifierSpan.ToString() : baseIdentifier,
            EntityType.Remote => baseIdentifier.StartsWith(RemotePrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"REMOTE:{identifierSpan}",
            EntityType.Sensor => baseIdentifier.StartsWith(SensorPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"SENSOR:{identifierSpan}{localSuffix}",
            EntityType.Switch => baseIdentifier.StartsWith(SwitchPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"SWITCH:{identifierSpan}{localSuffix}",
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, message: null)
        };
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
        var identifierMemory = GetBaseIdentifier(baseIdentifier);

        if (DisableEntityIdPrefixing)
            return baseIdentifier.Length != identifierMemory.Length ? identifierMemory : baseIdentifier;

        var localSuffix = string.IsNullOrWhiteSpace(suffix) ? string.Empty : $"_{suffix}";

        return entityType switch
        {
            EntityType.Cover => baseIdentifier.Span.StartsWith(CoverPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"COVER:{identifierMemory}{localSuffix}".AsMemory(),
            EntityType.Button => baseIdentifier.Span.StartsWith(ButtonPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"BUTTON:{identifierMemory}{localSuffix}".AsMemory(),
            EntityType.Climate => baseIdentifier.Span.StartsWith(ClimatePrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"CLIMATE:{identifierMemory}{localSuffix}".AsMemory(),
            EntityType.Light => baseIdentifier.Span.StartsWith(LightPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"LIGHT:{identifierMemory}{localSuffix}".AsMemory(),
            EntityType.MediaPlayer => baseIdentifier.Length != identifierMemory.Length ? identifierMemory : baseIdentifier,
            EntityType.Remote => baseIdentifier.Span.StartsWith(RemotePrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"REMOTE:{identifierMemory}".AsMemory(),
            EntityType.Sensor => baseIdentifier.Span.StartsWith(SensorPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"SENSOR:{identifierMemory}{localSuffix}".AsMemory(),
            EntityType.Switch => baseIdentifier.Span.StartsWith(SwitchPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"SWITCH:{identifierMemory}{localSuffix}".AsMemory(),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, message: null)
        };
    }

    private const string CoverPrefix = "COVER:";
    private const string ButtonPrefix = "BUTTON:";
    private const string ClimatePrefix = "CLIMATE:";
    private const string LightPrefix = "LIGHT:";
    private const string RemotePrefix = "REMOTE:";
    private const string SensorPrefix = "SENSOR:";
    private const string SwitchPrefix = "SWITCH:";

    private static readonly FrozenSet<string> PrefixesSet =
    [
        CoverPrefix, ButtonPrefix, ClimatePrefix, LightPrefix, RemotePrefix, SensorPrefix, SwitchPrefix
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
}