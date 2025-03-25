using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(1489474099632)]
public class ContainerPackPriceMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ContainerUserItemTemplate();
}
