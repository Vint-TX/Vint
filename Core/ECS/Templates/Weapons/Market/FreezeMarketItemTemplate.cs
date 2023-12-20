using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1433406790844)]
public class FreezeMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new FreezeUserItemTemplate();
}