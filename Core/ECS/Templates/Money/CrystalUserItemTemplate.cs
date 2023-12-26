using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Money;

[ProtocolId(1479898249156)]
public class CrystalUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new CrystalMarketItemTemplate();
}