using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Gold;

[ProtocolId(1530005871302)]
public class GoldBonusMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GoldBonusUserItemTemplate();
}