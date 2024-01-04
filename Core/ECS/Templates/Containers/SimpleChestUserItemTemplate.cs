using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Containers;

[ProtocolId(1543968970810)]
public class SimpleChestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new DonutChestMarketItemTemplate();
}