using Vint.Core.Items.Templates.Weapons.User;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.Market;

[ProtocolId(1435139147319)]
public class ThunderMarketItemTemplate : WeaponMarketItemTemplate {
    public override WeaponUserItemTemplate UserTemplate => new ThunderUserItemTemplate();
}
