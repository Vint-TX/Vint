using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1476707093577)]
public class QuestExpireDateComponent(
    DateTimeOffset date
) : IComponent {
    public DateTimeOffset Date { get; set; } = date;
}
