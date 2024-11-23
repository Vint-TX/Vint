using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Premium;

[ProtocolId(1513580884352)]
public class PremiumBoostUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new PremiumBoostMarketItemTemplate();
}
