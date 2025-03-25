using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Templates;

[ProtocolId(1491539827448)]
public class XCrystalUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new XCrystalMarketItemTemplate();
}
