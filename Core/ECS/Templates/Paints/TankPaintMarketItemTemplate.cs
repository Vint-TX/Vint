using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Paints;

[ProtocolId(1436443339132)]
public class TankPaintMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new TankPaintUserItemTemplate();
}