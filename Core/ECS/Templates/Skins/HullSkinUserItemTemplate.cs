using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Skins;

[ProtocolId(1469607958560)]
public class HullSkinUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new HullSkinMarketItemTemplate();
}