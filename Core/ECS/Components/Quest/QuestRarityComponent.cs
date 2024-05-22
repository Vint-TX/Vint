using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1495190227237)]
public class QuestRarityComponent(
    QuestRarityType rarityType
) : IComponent {
    public QuestRarityType RarityType { get; private set; } = rarityType;
}
