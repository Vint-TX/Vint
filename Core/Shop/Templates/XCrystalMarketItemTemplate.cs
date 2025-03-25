using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Templates;

[ProtocolId(1491539852367)]
public class XCrystalMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new XCrystalUserItemTemplate();
}
