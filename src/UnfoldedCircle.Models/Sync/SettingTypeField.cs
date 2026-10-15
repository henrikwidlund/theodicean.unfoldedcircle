namespace UnfoldedCircle.Models.Sync;

[JsonDerivedType(typeof(SettingTypeLabel))]
[JsonDerivedType(typeof(SettingTypeDropdown))]
[JsonDerivedType(typeof(SettingTypeCheckbox))]
[JsonDerivedType(typeof(SettingTypePassword))]
[JsonDerivedType(typeof(SettingTypeTextArea))]
[JsonDerivedType(typeof(SettingTypeNumber))]
[JsonDerivedType(typeof(SettingTypeText))]
public closed record SettingTypeField;

public sealed record SettingTypeLabel : SettingTypeField
{
    [JsonPropertyName("label")]
    public required SettingTypeLabelItem Label { get; init; }
}

public sealed record SettingTypeLabelItem
{
    [JsonPropertyName("value")]
    public required Dictionary<string, string> Value { get; init; }
}

public sealed record SettingTypeDropdown : SettingTypeField
{
    [JsonPropertyName("dropdown")]
    public required SettingTypeDropdownInner Dropdown { get; init; }
}

public sealed record SettingTypeDropdownInner
{
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("items")]
    public required SettingTypeDropdownItem[] Items { get; init; }
}

public sealed record SettingTypeDropdownItem
{
    /// <summary>
    /// Selection identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Value { get; init; }

    [JsonPropertyName("label")]
    public required Dictionary<string, string> Label { get; init; }
}

public sealed record SettingTypeCheckbox : SettingTypeField
{
    [JsonPropertyName("checkbox")]
    public required SettingTypeCheckboxInner Checkbox { get; init; }
}

public sealed record SettingTypeCheckboxInner
{
    /// <summary>
    /// Initial setting.
    /// </summary>
    [JsonPropertyName("value")]
    public required bool Value { get; init; }
}

public sealed record SettingTypePassword : SettingTypeField
{
    [JsonPropertyName("password")]
    public ValueRegex? Password { get; init; }
}

public sealed record SettingTypeTextArea : SettingTypeField
{
    [JsonPropertyName("textarea")]
    public required SettingTypeTextAreaInner TextArea { get; init; }
}

public sealed record SettingTypeTextAreaInner
{
    /// <summary>
    /// Optional default value.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

public sealed record SettingTypeNumber : SettingTypeField
{
    [JsonPropertyName("number")]
    public required SettingTypeNumberInner Number { get; init; }
}

public sealed record SettingTypeNumberInner
{
    [JsonPropertyName("value")]
    public required double Value { get; init; }

    [JsonPropertyName("min")]
    public double? Min { get; init; }

    [JsonPropertyName("max")]
    public double? Max { get; init; }

    [JsonPropertyName("steps")]
    public double? Steps { get; init; }

    [JsonPropertyName("decimals")]
    public ushort? Decimals { get; init; }

    [JsonPropertyName("unit")]
    public Dictionary<string, string>? Unit { get; init; }
}

public sealed record SettingTypeText : SettingTypeField
{
    [JsonPropertyName("text")]
    public required ValueRegex Text { get; init; }
}
