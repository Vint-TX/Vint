using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Skins;

[ProtocolId(1469607756132)]
public class WeaponSkinUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new WeaponSkinMarketItemTemplate();
}
