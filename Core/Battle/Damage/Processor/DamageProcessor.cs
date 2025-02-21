using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Damage.Processor;

public class DamageProcessor : IDamageProcessor {
    public async Task Damage(BattleTank source, BattleTank target, IEntity marketWeapon, IEntity battleWeapon, CalculatedDamage damage) {
        if (damage.Value <= 0) return;

        DamageType type = await Damage(target, damage);
        IPlayerConnection sourcePlayerConnection = source.Tanker.Connection;
        source.DealtDamage += damage.Value;
        await target.Tanker.Send(new DamageInfoTargetEvent(), battleWeapon, target.Entities.Tank);

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type) {
            case DamageType.Kill:
                if (!target.KillAssistants.TryAdd(source, damage.Value))
                    target.KillAssistants[source] += damage.Value;

                if (source == target)
                    await target.SelfDestruct();
                else
                    await target.KillBy(source, marketWeapon);
                break;

            case DamageType.Normal:
                if (!target.KillAssistants.TryAdd(source, damage.Value))
                    target.KillAssistants[source] += damage.Value;
                break;

            case DamageType.Critical:
                await sourcePlayerConnection.Send(new CriticalDamageEvent(target.Entities.Tank, damage.HitPoint), source.Entities.Weapon);

                if (!target.KillAssistants.TryAdd(source, damage.Value))
                    target.KillAssistants[source] += damage.Value;
                break;
        }

        await sourcePlayerConnection.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsSpecial),
            target.Entities.Tank);
    }

    public async Task<DamageType> Damage(BattleTank target, CalculatedDamage damage) {
        if (damage.Value <= 0)
            return DamageType.Normal;

        await target.SetHealth(target.Health - damage.Value);
        target.TakenDamage += damage.Value;

        return target.Health switch {
            <= 0 => DamageType.Kill,
            _ => damage.IsCritical
                ? DamageType.Critical
                : DamageType.Normal
        };
    }

    public async Task Heal(BattleTank source, BattleTank target, CalculatedDamage damage) {
        if (damage.Value <= 0) return;

        await Heal(target, damage);
        await source.Tanker.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsSpecial,
                true),
            target.Entities.Tank);
    }

    public async Task Heal(BattleTank target, CalculatedDamage damage) {
        if (damage.Value <= 0) return;

        float healed = Math.Min(target.MaxHealth - target.Health, damage.Value);

        await target.SetHealth(target.Health + damage.Value);
        target.TotalHealth += healed;
        await target.Tanker.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsSpecial,
                true),
            target.Entities.Tank);
    }
}
