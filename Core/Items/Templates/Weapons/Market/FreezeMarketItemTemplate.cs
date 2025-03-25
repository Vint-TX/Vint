using Vint.Core.Items.Templates.Weapons.User;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.Market;

[ProtocolId(1433406790844)]
public class FreezeMarketItemTemplate : WeaponMarketItemTemplate {
    public override WeaponUserItemTemplate UserTemplate => new FreezeUserItemTemplate();
}
