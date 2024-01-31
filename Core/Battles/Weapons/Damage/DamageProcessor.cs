using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.Server;

namespace Vint.Core.Battles.Weapons.Damage;

public interface IDamageProcessor {
    public Battle Battle { get; }

    public void Damage(BattleTank source, BattleTank target, IEntity weapon, CalculatedDamage damage);

    public DamageType Damage(BattleTank target, CalculatedDamage damage);

    public void Heal(BattleTank source, BattleTank target, float heal);

    public void Heal(BattleTank target, float heal);
}

public class DamageProcessor(
    Battle battle
) : IDamageProcessor {
    public Battle Battle { get; } = battle;

    public void Damage(BattleTank source, BattleTank target, IEntity weapon, CalculatedDamage damage) { // todo modules, statistics
        if (damage.Value < 0) return;

        DamageType type = Damage(target, damage);
        IPlayerConnection sourcePlayerConnection = source.BattlePlayer.PlayerConnection;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type) {
            case DamageType.Kill:
                if (source == target)
                    target.SelfDestruct(true);
                else
                    target.KillBy(source, weapon);
                break;

            case DamageType.Critical:
                sourcePlayerConnection.Send(new CriticalDamageEvent(target.Tank, damage.HitPoint), source.Weapon);
                break;
        }

        sourcePlayerConnection.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsBackHit || damage.IsTurretHit),
            target.Tank);
    }

    public DamageType Damage(BattleTank target, CalculatedDamage damage) {
        if (damage.Value < 0) return DamageType.Normal;

        target.SetHealth(target.Health - damage.Value);

        return target.Health switch {
            <= 0 => DamageType.Kill,
            _ => damage.IsCritical ? DamageType.Critical : DamageType.Normal
        };
    }

    public void Heal(BattleTank source, BattleTank target, float heal) => throw new NotImplementedException();

    public void Heal(BattleTank target, float heal) => throw new NotImplementedException();
}