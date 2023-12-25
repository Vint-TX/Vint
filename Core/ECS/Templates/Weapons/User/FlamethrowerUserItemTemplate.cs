using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1435139307697)]
public class FlamethrowerUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new FlamethrowerMarketItemTemplate();
}