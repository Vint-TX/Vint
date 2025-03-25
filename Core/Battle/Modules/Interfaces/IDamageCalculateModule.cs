using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.Battle.Weapons.Handlers;

namespace Vint.Core.Battle.Modules.Interfaces;

public interface IDamageCalculateModule {
    Task CalculatingDamage(BattleTank source, BattleTank target, IWeaponHandler weaponHandler);
}
