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
        string value = dictionary.GetValueOrDefault(key, defaultValue);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// Gets the identifier for a given entity type, ensuring it has the correct prefix.
    /// </summary>
    /// <param name="baseIdentifier">The base value for the identifier.</param>
    /// <param name="entityType">The <see cref="EntityType"/>.</param>
    /// <returns>A prefixed identifier based on the <paramref name="entityType"/> value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An unknown value for <see cref="EntityType"/> was specified.</exception>
    public static string GetIdentifier(this string baseIdentifier, in EntityType entityType)
    {
        var identifierSpan = baseIdentifier.AsSpan();
        foreach (string se in PrefixesSet)
        {
            if (identifierSpan.StartsWith(se, StringComparison.OrdinalIgnoreCase))
            {
                identifierSpan = identifierSpan[se.Length..];
                break;
            }
        }

        if (DisableEntityIdPrefixing)
            return baseIdentifier.Length != identifierSpan.Length ? identifierSpan.ToString() : baseIdentifier;

        return entityType switch
        {
            EntityType.Cover => baseIdentifier.StartsWith(CoverPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"COVER:{identifierSpan}",
            EntityType.Button => baseIdentifier.StartsWith(ButtonPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"BUTTON:{identifierSpan}",
            EntityType.Climate => baseIdentifier.StartsWith(ClimatePrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"CLIMATE:{identifierSpan}",
            EntityType.Light => baseIdentifier.StartsWith(LightPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"LIGHT:{identifierSpan}",
            EntityType.MediaPlayer => baseIdentifier.Length != identifierSpan.Length ? identifierSpan.ToString() : baseIdentifier,
            EntityType.Remote => baseIdentifier.StartsWith(RemotePrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"REMOTE:{identifierSpan}",
            EntityType.Sensor => baseIdentifier.StartsWith(SensorPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"SENSOR:{identifierSpan}",
            EntityType.Switch => baseIdentifier.StartsWith(SwitchPrefix, StringComparison.OrdinalIgnoreCase) ? baseIdentifier : $"SWITCH:{identifierSpan}",
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
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
    /// <returns>
    /// A prefixed identifier based on the <paramref name="entityType"/> value,
    /// or null if the <paramref name="baseIdentifier"/> is null or whitespace.
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string? GetNullableIdentifier(this string? baseIdentifier, in EntityType entityType)
        => string.IsNullOrWhiteSpace(baseIdentifier) ? null : baseIdentifier.GetIdentifier(entityType);

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
    /// <returns>The base identifier, or null if <paramref name="identifier"/> is null or whitespace.</returns>
    public static string? GetNullableBaseIdentifier(this string? identifier) => identifier.GetNullableIdentifier(EntityType.MediaPlayer);
}