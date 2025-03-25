using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(636458162767978928)]
public class DailyBonusReceivedEvent(
    int code
) : IEvent {
    public int Code { get; } = code;
}
