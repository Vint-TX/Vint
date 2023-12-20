using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138823086)]
public class HammerUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new HammerMarketItemTemplate();
}