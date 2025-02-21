using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Damage.Processor;

public interface IDamageProcessor {
    Task Damage(BattleTank source, BattleTank target, IEntity marketWeapon, IEntity battleWeapon, CalculatedDamage damage);

    Task<DamageType> Damage(BattleTank target, CalculatedDamage damage);

    Task Heal(BattleTank source, BattleTank target, CalculatedDamage heal);

    Task Heal(BattleTank target, CalculatedDamage heal);
}
