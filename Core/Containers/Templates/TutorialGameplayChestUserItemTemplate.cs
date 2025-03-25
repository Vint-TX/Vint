using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(636413315444096863)]
public class TutorialGameplayChestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new TutorialGameplayChestMarketItemTemplate();
}
