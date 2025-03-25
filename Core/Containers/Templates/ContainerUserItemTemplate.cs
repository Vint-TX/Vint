using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(1479807574456)]
public class ContainerUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ContainerPackPriceMarketItemTemplate();
}
