namespace UnfoldedCircle.Models.Sync;

public record SetDriverUserDataMsgData
{
    [JsonPropertyName("input_values")]
    public Dictionary<string, string>? InputValues { get; set; }

    [JsonPropertyName("confirm")]
    public bool? Confirm { get; init; }
}