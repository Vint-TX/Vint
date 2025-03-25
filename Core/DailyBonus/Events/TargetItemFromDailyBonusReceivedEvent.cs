using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(636464291410970703)]
public class TargetItemFromDailyBonusReceivedEvent(
    long detailMarketItemId
) : IEvent {
    public long DetailMarketItemId { get; } = detailMarketItemId;
}
