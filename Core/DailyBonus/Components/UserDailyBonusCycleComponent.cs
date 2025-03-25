using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Components;

[ProtocolId(636459034861529826)]
public class UserDailyBonusCycleComponent(
    int cycleNumber
) : IComponent {
    public int CycleNumber { get; set; } = cycleNumber;
}
