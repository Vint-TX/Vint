using Vint.Core.ECS.Events;

namespace Vint.Core.Battle.Common.Events;

public abstract class TimeEvent : IEvent {
    public int ClientTime { get; set; }
}
