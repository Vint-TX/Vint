using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.User;

[ProtocolId(1433406776150)]
public class SmokyUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new SmokyMarketItemTemplate();
}