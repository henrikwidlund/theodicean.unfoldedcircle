namespace UnfoldedCircle.Server;

internal static class EnvironmentConstants
{
    public static readonly int MaxConcurrency = Environment.ProcessorCount * 2;
}