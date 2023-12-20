using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Skins;

[ProtocolId(1469607574709)]
public class WeaponSkinMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new WeaponSkinUserItemTemplate();
}