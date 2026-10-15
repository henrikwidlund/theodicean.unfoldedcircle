namespace UnfoldedCircle.Models.Events;

public sealed record IntegrationSetupError
{
    [JsonPropertyName("error")]
    public required IntegrationSetupErrorCode Error { get; init; }
}
