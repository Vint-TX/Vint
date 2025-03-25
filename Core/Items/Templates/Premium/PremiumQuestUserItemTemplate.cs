using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Premium;

[ProtocolId(1513582138852)]
public class PremiumQuestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new PremiumQuestMarketItemTemplate();
}
