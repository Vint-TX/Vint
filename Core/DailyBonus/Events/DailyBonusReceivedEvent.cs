using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.DailyBonus;

[ProtocolId(636458162767978928)]
public class DailyBonusReceivedEvent(
    int code
) : IEvent {
    public int Code { get; } = code;
}
