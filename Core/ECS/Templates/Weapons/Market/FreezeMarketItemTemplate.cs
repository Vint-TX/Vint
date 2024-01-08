using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1433406790844)]
public class FreezeMarketItemTemplate : WeaponMarketItemTemplate {
    public override WeaponUserItemTemplate UserTemplate => new FreezeUserItemTemplate();
}