using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435139182866)]
public class ThunderUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ThunderMarketItemTemplate();
}