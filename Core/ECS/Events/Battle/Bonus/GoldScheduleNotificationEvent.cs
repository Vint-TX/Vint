using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Bonus;

[ProtocolId(1430205112111)]
public class GoldScheduleNotificationEvent(
    string sender
) : IEvent {
    public string Sender { get; private set; } = sender;
}