using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1433406776150)]
public class SmokyUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new SmokyMarketItemTemplate();
}