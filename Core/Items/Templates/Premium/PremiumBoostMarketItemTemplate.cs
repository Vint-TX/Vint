using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Premium;

[ProtocolId(1513580195801)]
public class PremiumBoostMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PremiumBoostUserItemTemplate();
}
