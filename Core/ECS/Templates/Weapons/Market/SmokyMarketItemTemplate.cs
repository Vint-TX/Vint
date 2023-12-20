using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1433406763806)]
public class SmokyMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new SmokyUserItemTemplate();
}