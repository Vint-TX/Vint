using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1493196614850)]
public class QuestRewardComponent(
    long id,
    int amount
) : IComponent {
    public Dictionary<long, int> Reward { get; set; } = new() { { id, amount } };
}
