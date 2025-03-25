using Vint.Core.Items.Templates.Weapons.User;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.Market;

[ProtocolId(1435138247945)]
public class RicochetMarketItemTemplate : WeaponMarketItemTemplate {
    public override WeaponUserItemTemplate UserTemplate => new RicochetUserItemTemplate();
}
