using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Premium;

[ProtocolId(1513582138852)]
public class PremiumQuestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new PremiumQuestMarketItemTemplate();
}