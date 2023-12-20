using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435910167704)]
public class IsisUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new IsisMarketItemTemplate();
}