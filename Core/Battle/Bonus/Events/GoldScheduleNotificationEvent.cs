using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Events;

[ProtocolId(1430205112111)]
public class GoldScheduleNotificationEvent(
    string sender
) : IEvent {
    public string Sender { get; private set; } = sender;
}
