using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Skins;

[ProtocolId(1469607756132)]
public class WeaponSkinUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new WeaponSkinMarketItemTemplate();
}
