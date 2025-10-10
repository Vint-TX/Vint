using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Autopilot.Components;

[ProtocolId(1508219865424)]
public class AutopilotMovementControllerComponent : IComponent {
    public bool Moving { get; set; }
    public bool MoveToTarget { get; set; }

    public IEntity? Target { get; set; }
    public Vector3 Destination { get; set; }
}
