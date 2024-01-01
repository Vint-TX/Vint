using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Gold;

[ProtocolId(1530005856940)]
public class GoldBonusUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new GoldBonusMarketItemTemplate();
}