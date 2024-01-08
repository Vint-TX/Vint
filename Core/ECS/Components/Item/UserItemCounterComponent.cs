using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Item;

[ProtocolId(1479807693001)]
public class UserItemCounterComponent(
    long count
) : IComponent {
    public long Count { get; set; } = count;
}