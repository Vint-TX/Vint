using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Hulls;

[ProtocolId(1438603503434)]
public class TankUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new TankMarketItemTemplate();
}