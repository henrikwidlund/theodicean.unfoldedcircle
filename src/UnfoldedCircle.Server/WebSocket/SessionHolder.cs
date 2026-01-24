using System.Collections.Concurrent;

namespace UnfoldedCircle.Server.WebSocket;

internal static class SessionHolder
{
    public static readonly ConcurrentDictionary<string, bool> SubscribeEventsMap = new(StringComparer.Ordinal);
    public static readonly ConcurrentDictionary<string, sbyte> SocketBroadcastingEvents = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, sbyte> EntityIdBroadcastingEvents = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, SetupStep> NextSetupSteps = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, string> ReconfigureEntityMap = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, CancellationTokenSource> CurrentRepeatCommandMap = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, HashSet<string>> SensorTypesMap = new(StringComparer.OrdinalIgnoreCase);
}