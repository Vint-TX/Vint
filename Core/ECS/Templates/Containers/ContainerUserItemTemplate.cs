using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(1479807574456)]
public class ContainerUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ContainerPackPriceMarketItemTemplate();
}