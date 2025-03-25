using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

[ProtocolId(8445798616771064825), ClientAddable, ClientChangeable, ClientRemovable]
public class ShaftAimingTargetPointComponent : IComponent {
    public bool IsInsideTankPart { get; private set; }
    public Vector3? Point { get; private set; }
}
