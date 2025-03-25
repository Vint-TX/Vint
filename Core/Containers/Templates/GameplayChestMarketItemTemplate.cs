using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(1487149202122)]
public class GameplayChestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GameplayChestUserItemTemplate();
}
