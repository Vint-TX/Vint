using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Money;

[ProtocolId(1491539827448)]
public class XCrystalUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new XCrystalMarketItemTemplate();
}