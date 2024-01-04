using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(636408122917164205)]
public class DonutChestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new SimpleChestUserItemTemplate();
}