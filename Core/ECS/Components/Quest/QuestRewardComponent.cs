using JetBrains.Annotations;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1493196614850)]
public class QuestRewardComponent : IComponent {
    [UsedImplicitly]
    public QuestRewardComponent() { }

    public QuestRewardComponent(long id, int amount) {
        Reward = id;
        Amount = amount;
    }

    public long Reward { get; set; }
    public int Amount { get; set; }
}
