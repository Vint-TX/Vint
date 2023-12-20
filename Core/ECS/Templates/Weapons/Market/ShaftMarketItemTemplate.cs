using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138382391)]
public class ShaftMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ShaftUserItemTemplate();
}