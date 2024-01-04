using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(636413290399070700)]
public class TutorialGameplayChestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new TutorialGameplayChestUserItemTemplate();
}