namespace UnfoldedCircle.Server.Configuration;

/// <summary>
/// Global configuration for the Unfolded Circle server. This includes settings that apply to the entire server and all entities.
/// </summary>
public record UnfoldedCircleGlobalConfiguration
{
    /// <summary>
    /// The maximum wait time for a received message to be handled before being canceled.
    /// Must not exceed 10 seconds as that is the timeout Core uses for responses.
    /// </summary>
    public double? MaxMessageHandlingWaitTimeInSeconds { get; init; }
}
