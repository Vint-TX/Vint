using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Components;

[ProtocolId(1494535525136)]
public class SlotIndexComponent(
    int index
) : IComponent {
    public int Index { get; private set; } = index;
}
