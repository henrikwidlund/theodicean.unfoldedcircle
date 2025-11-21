using UnfoldedCircle.Models.Sync;

namespace UnfoldedCircle.Server.Json;

/// <summary>
/// Converter for <see cref="SettingTypeField"/> that determines the specific type based on the JSON properties.
/// </summary>
public class SettingTypeFieldConverter : JsonConverter<SettingTypeField>
{
    /// <inheritdoc />
    public override SettingTypeField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        return root switch {
            _ when root.TryGetProperty("label", out _) => root.Deserialize<SettingTypeLabel>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypeLabel)!,
            _ when root.TryGetProperty("dropdown", out _) => root.Deserialize<SettingTypeDropdown>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypeDropdown)!,
            _ when root.TryGetProperty("checkbox", out _) => root.Deserialize<SettingTypeCheckbox>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypeCheckbox)!,
            _ when root.TryGetProperty("password", out _) => root.Deserialize<SettingTypePassword>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypePassword)!,
            _ when root.TryGetProperty("textarea", out _) => root.Deserialize<SettingTypeTextArea>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypeTextArea)!,
            _ when root.TryGetProperty("number", out _) => root.Deserialize<SettingTypeNumber>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypeNumber)!,
            _ when root.TryGetProperty("text", out _) => root.Deserialize<SettingTypeText>(UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters.SettingTypeText)!,
            _ => throw new JsonException("Unknown setting type field.")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SettingTypeField value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), UnfoldedCircleJsonSerializerContext.InstanceWithoutCustomConverters);
}