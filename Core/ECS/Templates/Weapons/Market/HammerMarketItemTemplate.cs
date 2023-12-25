using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435138774158)]
public class HammerMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new HammerUserItemTemplate();
}