namespace UnfoldedCircle.Models;

/// <summary>
/// A <see cref="JsonStringEnumConverter{TEnum}"/> that disallows integer representation.
/// </summary>
/// <typeparam name="TEnum">The enum type that this converter targets.</typeparam>
public sealed class StrictEnumConverter<TEnum>() : JsonStringEnumConverter<TEnum>(allowIntegerValues: false)
    where TEnum : struct, Enum;
