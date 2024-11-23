using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Premium;

[ProtocolId(1513580195801)]
public class PremiumBoostMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PremiumBoostUserItemTemplate();
}
