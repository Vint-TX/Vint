using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Paints;

[ProtocolId(1438603647557)]
public class TankPaintUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new TankPaintMarketItemTemplate();
}
