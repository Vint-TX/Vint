using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435910107339)]
public class IsisMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new IsisUserItemTemplate();
}