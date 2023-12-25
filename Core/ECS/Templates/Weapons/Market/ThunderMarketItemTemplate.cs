using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435139147319)]
public class ThunderMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ThunderUserItemTemplate();
}