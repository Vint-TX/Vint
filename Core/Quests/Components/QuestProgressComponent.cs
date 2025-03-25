using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Components;

[ProtocolId(1476091404409)]
public class QuestProgressComponent(
    float currentValue,
    float targetValue
) : IComponent {
    public float PrevValue { get; set; } = currentValue;
    public float CurrentValue { get; set; } = currentValue;
    public float TargetValue { get; private set; } = targetValue;
    public bool PrevComplete { get; set; }
    public bool CurrentComplete { get; set; }
}
