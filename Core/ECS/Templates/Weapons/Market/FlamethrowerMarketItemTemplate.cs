using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435138131935)]
public class FlamethrowerMarketItemTemplate : WeaponMarketItemTemplate {
    public override WeaponUserItemTemplate UserTemplate => new FlamethrowerUserItemTemplate();
}