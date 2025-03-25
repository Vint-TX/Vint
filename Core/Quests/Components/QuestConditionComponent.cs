using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Components;

[ProtocolId(1493901546731)]
public class QuestConditionComponent(
    QuestConditionType type,
    long value
) : IComponent {
    public QuestConditionComponent(BattleType battleType) : this(QuestConditionType.Mode, (long)battleType) { }

    public Dictionary<QuestConditionType, long> Condition { get; private set; } = new(1) { { type, value } };
}
