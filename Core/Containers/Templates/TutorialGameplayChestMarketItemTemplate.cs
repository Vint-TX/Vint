using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(636413290399070700)]
public class TutorialGameplayChestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new TutorialGameplayChestUserItemTemplate();
}
