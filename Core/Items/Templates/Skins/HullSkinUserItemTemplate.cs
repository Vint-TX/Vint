using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Skins;

[ProtocolId(1469607958560)]
public class HullSkinUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new HullSkinMarketItemTemplate();
}
