using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(1486562494879)]
public class GameplayChestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new GameplayChestMarketItemTemplate();
}
