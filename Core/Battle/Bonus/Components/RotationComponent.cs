using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Components;

[ProtocolId(-1853333282151870933)]
public class RotationComponent(
    Vector3 rotationEuler
) : IComponent {
    public Vector3 RotationEuler { get; private set; } = rotationEuler;
}
