using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Quests.Components;

[ProtocolId(1516789840617)]
public class BattleQuestTargetComponent : IComponent {
    public int TargetValue { get; private set; }
}
