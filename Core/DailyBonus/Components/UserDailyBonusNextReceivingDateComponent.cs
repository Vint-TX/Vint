using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.DailyBonus;

[ProtocolId(636462622709176439)]
public class UserDailyBonusNextReceivingDateComponent(
    DateTimeOffset? date,
    TimeSpan interval
) : IComponent {
    public UserDailyBonusNextReceivingDateComponent(DateTimeOffset nextReceiveTime, DateTimeOffset lastReceiveTime)
        : this(nextReceiveTime, (nextReceiveTime - lastReceiveTime).Duration()) { }

    public DateTimeOffset? Date { get; } = date;
    [ProtocolName("TotalMillisLength")] public long IntervalMs { get; } = (long)interval.TotalMilliseconds;
}
