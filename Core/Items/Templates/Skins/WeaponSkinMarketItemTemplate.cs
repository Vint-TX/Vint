using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Skins;

[ProtocolId(1469607574709)]
public class WeaponSkinMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new WeaponSkinUserItemTemplate();
}
