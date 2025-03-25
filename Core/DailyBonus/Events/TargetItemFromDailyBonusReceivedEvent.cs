using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.DailyBonus;

[ProtocolId(636464291410970703)]
public class TargetItemFromDailyBonusReceivedEvent(
    long detailMarketItemId
) : IEvent {
    public long DetailMarketItemId { get; } = detailMarketItemId;
}
