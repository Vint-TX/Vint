using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public abstract class StreamWeaponHandler : TankWeaponHandler, ITemperatureWeaponHandler {
    protected StreamWeaponHandler(BattleTank battleTank) : base(battleTank) {
        Cooldown = TimeSpan.FromSeconds(ConfigManager.GetComponent<WeaponCooldownComponent>(BattleConfigPath).CooldownIntervalSec);
        DamagePerSecond = ConfigManager.GetComponent<DamagePerSecondPropertyComponent>(MarketConfigPath).FinalValue;
    }

    public float DamagePerSecond { get; }
    public Dictionary<long, DateTimeOffset> IncarnationIdToHitTime { get; } = new();

    public abstract float TemperatureLimit { get; }
    public abstract float TemperatureDelta { get; }

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId)) return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        await targetTank.UpdateTemperatureAssists(BattleTank, this, !isEnemy);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    protected bool IsCooldownActive(long targetIncarnationId) {
        if (!IncarnationIdToHitTime.TryGetValue(targetIncarnationId, out DateTimeOffset lastHitTime)) {
            IncarnationIdToHitTime[targetIncarnationId] = DateTimeOffset.UtcNow;
            return true;
        }

        if (DateTimeOffset.UtcNow - lastHitTime < Cooldown) return true;

        IncarnationIdToHitTime.Remove(targetIncarnationId);
        return false;
    }

    public virtual ValueTask Reset() =>
        BattleTank.BattlePlayer.PlayerConnection.Send(new StreamWeaponResetStateEvent(), BattleEntity);
}
