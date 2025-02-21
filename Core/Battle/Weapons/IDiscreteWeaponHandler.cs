namespace Vint.Core.Battle.Weapons;

public interface IDiscreteWeaponHandler : IWeaponHandler {
    float MinDamage { get; }
    float MaxDamage { get; }
}
