using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Covers;

[ProtocolId(636287154959625373)]
public class WeaponPaintUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new WeaponPaintMarketItemTemplate();
}
