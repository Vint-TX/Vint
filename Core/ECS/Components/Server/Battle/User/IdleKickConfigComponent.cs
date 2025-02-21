namespace Vint.Core.ECS.Components.Server.Battle.User;

public class IdleKickConfigComponent : IComponent {
    public int IdleKickTimeSec { get; private set; }
    public int IdleWarningTimeSec { get; private set; }
    public int CheckPeriodicTimeSec { get; private set; }
}
