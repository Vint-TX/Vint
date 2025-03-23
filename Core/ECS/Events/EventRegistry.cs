using System.Collections.Concurrent;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events;

public static class EventRegistry {
    static ConcurrentDictionary<long, IEvent> Events { get; } = [];

    public static TEvent GetOrAdd<TEvent>() where TEvent : IEvent, new() {
        long id = typeof(TEvent).GetProtocolId();
        return (TEvent)Events.GetOrAdd(id, _ => new TEvent());
    }
}
