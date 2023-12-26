using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Premium;

[ProtocolId(1513580238036)]
public class PremiumQuestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PremiumQuestUserItemTemplate();
}