using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Premium;

[ProtocolId(1513580195801)]
public class PremiumBoostMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PremiumBoostUserItemTemplate();
}