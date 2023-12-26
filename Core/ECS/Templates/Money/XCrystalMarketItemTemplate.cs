using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Money;

[ProtocolId(1491539852367)]
public class XCrystalMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new XCrystalUserItemTemplate();
}