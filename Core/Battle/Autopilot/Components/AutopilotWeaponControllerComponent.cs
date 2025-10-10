using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Autopilot.Components;

[ProtocolId(1508220592078), ClientChangeable]
public class AutopilotWeaponControllerComponent : IComponent {
    public bool Attack { get; set; }
    public bool TargetAchievable { get; private set; }
    public float Accuracy { get; set; }
}
