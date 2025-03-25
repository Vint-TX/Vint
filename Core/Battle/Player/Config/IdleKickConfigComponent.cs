using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Player.Config;

public class IdleKickConfigComponent : IComponent {
    public int IdleKickTimeSec { get; private set; }
    public int IdleWarningTimeSec { get; private set; }
    public int CheckPeriodicTimeSec { get; private set; }
}
