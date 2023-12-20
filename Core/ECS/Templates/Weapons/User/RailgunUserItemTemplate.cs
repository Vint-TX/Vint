using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138575888)]
public class RailgunUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new RailgunMarketItemTemplate();
}