using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Hulls;

[ProtocolId(1433406732656)]
public class TankMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new TankUserItemTemplate();
}