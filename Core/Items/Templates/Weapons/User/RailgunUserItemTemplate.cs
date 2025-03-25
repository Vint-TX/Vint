using Vint.Core.Items.Templates.Weapons.Market;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.User;

[ProtocolId(1435138575888)]
public class RailgunUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new RailgunMarketItemTemplate();
}
