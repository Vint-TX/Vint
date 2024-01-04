using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(1486562494879)]
public class GameplayChestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new GameplayChestMarketItemTemplate();
}