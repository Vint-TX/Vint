using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Paints;

[ProtocolId(1438603647557)]
public class TankPaintUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new TankPaintMarketItemTemplate();
}
