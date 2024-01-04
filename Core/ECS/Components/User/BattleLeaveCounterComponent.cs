using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1502092676956)]
public class BattleLeaveCounterComponent(
    long value,
    int needGoodBattles
) : IComponent {
    public long Value { get; set; } = value;
    public int NeedGoodBattles { get; set; } = needGoodBattles;
}