using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435139228955)]
public class VulcanUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new VulcanMarketItemTemplate();
}