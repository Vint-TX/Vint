using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1435138683277)]
public class TwinsUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new TwinsMarketItemTemplate();
}