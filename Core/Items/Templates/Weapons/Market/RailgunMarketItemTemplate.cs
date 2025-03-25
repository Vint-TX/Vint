using Vint.Core.Items.Templates.Weapons.User;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.Market;

[ProtocolId(1435138450574)]
public class RailgunMarketItemTemplate : WeaponMarketItemTemplate {
    public override WeaponUserItemTemplate UserTemplate => new RailgunUserItemTemplate();
}
