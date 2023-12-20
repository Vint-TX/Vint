using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1433406804439)]
public class FreezeUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new FreezeMarketItemTemplate();
}