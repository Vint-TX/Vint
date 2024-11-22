namespace Vint.Core.Battles.Weapons;

public interface IDiscreteWeaponHandler : IWeaponHandler {
    float MinDamage { get; }
    float MaxDamage { get; }
}
