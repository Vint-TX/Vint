namespace Vint.Core.Battle.Weapons.Handlers;

public interface IDiscreteWeaponHandler : IWeaponHandler {
    float MinDamage { get; }
    float MaxDamage { get; }
}
