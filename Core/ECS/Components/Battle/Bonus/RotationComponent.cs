using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Bonus;

[ProtocolId(-1853333282151870933)]
public class RotationComponent(
    Vector3 rotationEuler
) : IComponent {
    public Vector3 RotationEuler { get; private set; } = rotationEuler;
}