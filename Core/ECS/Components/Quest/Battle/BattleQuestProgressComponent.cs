using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest.Battle;

[ProtocolId(1516709775798)]
public class BattleQuestProgressComponent(
    int currentValue
) {
    public int CurrentValue { get; private set; } = currentValue;
}
