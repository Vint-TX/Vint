using Vint.Core.Items.Templates.Weapons.Market;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.User;

[ProtocolId(1433406776150)]
public class SmokyUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new SmokyMarketItemTemplate();
}
