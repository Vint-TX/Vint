using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Components;

[ProtocolId(1473253631059)]
public class GoodsXPriceComponent : IComponent {
    public long Price { get; private set; }
}
