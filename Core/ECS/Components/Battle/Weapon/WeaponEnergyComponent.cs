using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(8236491228938594733)]
public class WeaponEnergyComponent(
    float energy
) : IComponent {
    public float Energy { get; set; } = energy;
}