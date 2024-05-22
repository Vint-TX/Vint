using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1494535525136)]
public class SlotIndexComponent(
    int index
) : IComponent {
    public int Index { get; private set; } = index;
}
