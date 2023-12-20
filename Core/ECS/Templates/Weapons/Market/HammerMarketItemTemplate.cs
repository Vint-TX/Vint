using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138774158)]
public class HammerMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new HammerUserItemTemplate();
}