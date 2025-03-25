using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Components;

[ProtocolId(1479807693001)]
public class UserItemCounterComponent(
    long count
) : IComponent {
    public long Count { get; set; } = count;
}
