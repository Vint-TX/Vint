using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(1487149202122)]
public class GameplayChestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GameplayChestUserItemTemplate();
}
