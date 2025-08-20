using System.Collections.Concurrent;

using Makaretu.Dns.Resolving;

namespace UnfoldedCircle.Server.WebSocket;

internal static class SessionHolder
{
    public static readonly ConcurrentDictionary<string, string> SocketIdEntityIpMap = new(StringComparer.Ordinal);
    public static readonly ConcurrentDictionary<string, bool> SubscribeEventsMap = new(StringComparer.Ordinal);
    public static readonly ConcurrentSet<string> BroadcastingEvents = new(StringComparer.OrdinalIgnoreCase);
}