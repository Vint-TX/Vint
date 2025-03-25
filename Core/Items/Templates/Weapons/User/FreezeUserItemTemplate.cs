using Vint.Core.Items.Templates.Weapons.Market;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.User;

[ProtocolId(1433406804439)]
public class FreezeUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new FreezeMarketItemTemplate();
}
