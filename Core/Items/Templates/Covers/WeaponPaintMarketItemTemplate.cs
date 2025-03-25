using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Covers;

[ProtocolId(636287153836461132)]
public class WeaponPaintMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new WeaponPaintUserItemTemplate();
}
