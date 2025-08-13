using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extensions for configuration handling in the Unfolded Circle server.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a value from the configuration or returns a default value if the key is not found or its value cannot be parsed.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> to read from.</param>
    /// <param name="key">The key for the setting.</param>
    /// <param name="defaultValue">The value to return if the setting isn't found or can't be parsed.</param>
    /// <typeparam name="T">The type the setting should be parsed as.</typeparam>
    public static T GetOrDefault<T>(this IConfiguration configuration, string key, T defaultValue)
        where T : IParsable<T> =>
        T.TryParse(configuration[key], CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
}