using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Templates;

[ProtocolId(1479898249156)]
public class CrystalUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new CrystalMarketItemTemplate();
}
