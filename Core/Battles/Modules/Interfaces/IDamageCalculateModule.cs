using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;

namespace Vint.Core.Battles.Modules.Interfaces;

public interface IDamageCalculateModule {
    Task CalculatingDamage(BattleTank source, BattleTank target, IWeaponHandler weaponHandler);
}
