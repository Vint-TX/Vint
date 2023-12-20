using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Skins;

[ProtocolId(1469607967377)]
public class HullSkinMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new HullSkinUserItemTemplate();
}