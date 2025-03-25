using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1502092676956)]
public class BattleLeaveCounterComponent(
    long value,
    int needGoodBattles
) : IComponent {
    public long Value { get; set; } = value;
    public int NeedGoodBattles { get; set; } = needGoodBattles;
}
