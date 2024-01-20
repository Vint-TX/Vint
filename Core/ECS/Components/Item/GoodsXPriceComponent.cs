using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Item;

[ProtocolId(1473253631059)]
public class GoodsXPriceComponent : IComponent {
    public long Price { get; private set; }
}