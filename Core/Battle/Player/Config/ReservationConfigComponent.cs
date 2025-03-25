using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Player.Config;

public class ReservationConfigComponent : IComponent {
    public int ReservationDurationTimeSec { get; private set; }
}
