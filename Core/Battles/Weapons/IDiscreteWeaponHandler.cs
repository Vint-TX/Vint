namespace Vint.Core.Battles.Weapons;

public interface IDiscreteWeaponHandler : IWeaponHandler {
    public float MinDamage { get; }
    public float MaxDamage { get; }
}