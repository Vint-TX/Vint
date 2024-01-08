using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1435139228955)]
public class VulcanUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new VulcanMarketItemTemplate();
}