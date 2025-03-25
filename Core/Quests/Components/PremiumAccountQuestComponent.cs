using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Components;

[ProtocolId(1513252653655)]
public class PremiumAccountQuestComponent(
    DateTimeOffset endDate
) : IComponent {
    public DateTimeOffset EndDate { get; private set; } = endDate;
}
