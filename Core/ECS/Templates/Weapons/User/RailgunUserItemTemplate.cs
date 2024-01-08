using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1435138575888)]
public class RailgunUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new RailgunMarketItemTemplate();
}