using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Components;

[ProtocolId(1476707093577)]
public class QuestExpireDateComponent(
    DateTimeOffset date
) : IComponent {
    public DateTimeOffset Date { get; set; } = date;
}
