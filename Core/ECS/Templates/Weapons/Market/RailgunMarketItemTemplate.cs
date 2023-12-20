using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138450574)]
public class RailgunMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new RailgunUserItemTemplate();
}