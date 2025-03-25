using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Skins;

[ProtocolId(1469607967377)]
public class HullSkinMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new HullSkinUserItemTemplate();
}
