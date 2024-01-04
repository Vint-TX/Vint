using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(1489474099632)]
public class ContainerPackPriceMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ContainerUserItemTemplate();
}