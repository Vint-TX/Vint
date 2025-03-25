using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Quests.Components;

[ProtocolId(1516709775798)]
public class BattleQuestProgressComponent(
    int currentValue
) {
    public int CurrentValue { get; private set; } = currentValue;
}
