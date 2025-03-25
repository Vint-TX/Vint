using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Templates;

[ProtocolId(1479898113503)]
public class CrystalMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new CrystalUserItemTemplate();
}
