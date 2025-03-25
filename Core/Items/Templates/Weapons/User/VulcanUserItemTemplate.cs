using Vint.Core.Items.Templates.Weapons.Market;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Weapons.User;

[ProtocolId(1435139228955)]
public class VulcanUserItemTemplate : WeaponUserItemTemplate {
    public override WeaponMarketItemTemplate MarketTemplate => new VulcanMarketItemTemplate();
}
