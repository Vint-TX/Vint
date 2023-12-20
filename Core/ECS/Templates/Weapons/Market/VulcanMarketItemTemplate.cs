using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138178392)]
public class VulcanMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new VulcanUserItemTemplate();
}