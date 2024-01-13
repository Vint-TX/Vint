namespace Vint.Core.ECS.Events.Battle;

public abstract class TimeEvent : IEvent {
    public int ClientTime { get; set; }
}