using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138415322)]
public class ShaftUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ShaftMarketItemTemplate();
}