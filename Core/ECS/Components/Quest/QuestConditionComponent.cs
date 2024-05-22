using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1493901546731)]
public class QuestConditionComponent(
    QuestConditionType type,
    long value
) : IComponent {
    public QuestConditionComponent(BattleType battleType) : this(QuestConditionType.Mode, (long)battleType) { }

    public Dictionary<QuestConditionType, long> Condition { get; private set; } = new(1) { { type, value } };
}
