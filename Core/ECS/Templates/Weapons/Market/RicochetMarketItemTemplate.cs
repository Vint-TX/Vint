using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435138247945)]
public class RicochetMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new RicochetUserItemTemplate();
}