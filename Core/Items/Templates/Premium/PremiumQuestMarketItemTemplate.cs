using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Premium;

[ProtocolId(1513580238036)]
public class PremiumQuestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PremiumQuestUserItemTemplate();
}
