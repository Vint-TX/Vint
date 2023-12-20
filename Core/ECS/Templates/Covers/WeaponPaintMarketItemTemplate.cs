using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Covers;

[ProtocolId(636287153836461132)]
public class WeaponPaintMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new WeaponPaintUserItemTemplate();
}