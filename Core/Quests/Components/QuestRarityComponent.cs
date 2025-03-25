using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Components;

[ProtocolId(1495190227237)]
public class QuestRarityComponent(
    QuestRarityType rarityType
) : IComponent {
    public QuestRarityType RarityType { get; private set; } = rarityType;
}
