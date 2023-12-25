using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435138450574)]
public class RailgunMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new RailgunUserItemTemplate();
}