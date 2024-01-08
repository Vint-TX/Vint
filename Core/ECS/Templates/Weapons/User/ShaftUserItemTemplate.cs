using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1435138415322)]
public class ShaftUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new ShaftMarketItemTemplate();
}