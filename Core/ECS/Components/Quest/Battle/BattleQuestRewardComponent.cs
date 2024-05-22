using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest.Battle;

[ProtocolId(1516873245609)]
public class BattleQuestRewardComponent : IComponent {
    public BattleQuestReward BattleQuestReward { get; private set; }
    public int Quantity { get; private set; }
}
