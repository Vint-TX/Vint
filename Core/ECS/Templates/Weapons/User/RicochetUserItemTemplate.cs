using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1435138285320)]
public class RicochetUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new RicochetMarketItemTemplate();
}