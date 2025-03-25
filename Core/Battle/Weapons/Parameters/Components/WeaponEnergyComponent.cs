using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(8236491228938594733)]
public class WeaponEnergyComponent(
    float energy
) : IComponent {
    public float Energy { get; set; } = energy;
}
