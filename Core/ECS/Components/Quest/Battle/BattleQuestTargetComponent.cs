using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest.Battle;

[ProtocolId(1516789840617)]
public class BattleQuestTargetComponent(
    int targetValue
) : IComponent {
    public int TargetValue { get; private set; } = targetValue;
}
