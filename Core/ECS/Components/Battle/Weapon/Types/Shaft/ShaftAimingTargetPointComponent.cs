using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(8445798616771064825), ClientAddable, ClientChangeable, ClientRemovable]
public class ShaftAimingTargetPointComponent : IComponent {
    public bool IsInsideTankPart { get; private set; }
    public Vector3? Point { get; private set; }
}