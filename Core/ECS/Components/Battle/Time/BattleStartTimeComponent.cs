using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Time;

[ProtocolId(1436521738148)]
public class BattleStartTimeComponent(
    DateTimeOffset? startTime
) : IComponent {
    public DateTimeOffset? RoundStartTime { get; set; } = startTime;
}