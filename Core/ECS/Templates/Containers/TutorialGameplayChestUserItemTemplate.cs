using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(636413315444096863)]
public class TutorialGameplayChestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new TutorialGameplayChestMarketItemTemplate();
}