using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Quests.Components;

[ProtocolId(1516873245609)]
public class BattleQuestRewardComponent : IComponent {
    public BattleQuestReward BattleQuestReward { get; private set; }
    public int Quantity { get; private set; }
}
