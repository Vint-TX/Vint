using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Money;

[ProtocolId(1479898113503)]
public class CrystalMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new CrystalUserItemTemplate();
}