using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Weapons.Damage;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public abstract class StreamWeaponHandler : WeaponHandler {
    protected StreamWeaponHandler(BattleTank battleTank) : base(battleTank) {
        Cooldown = TimeSpan.FromSeconds(ConfigManager.GetComponent<WeaponCooldownComponent>(BattleConfigPath).CooldownIntervalSec);
        DamagePerSecond = ConfigManager.GetComponent<DamagePerSecondPropertyComponent>(MarketConfigPath).FinalValue;
    }

    public float DamagePerSecond { get; }
    public Dictionary<long, DateTimeOffset> IncarnationIdToHitTime { get; } = new();

    public override void Fire(HitTarget target) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId)) return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        // ReSharper disable once ArrangeRedundantParentheses
        if (targetTank.StateManager.CurrentState is not Active ||
            (!isEnemy && !battle.Properties.FriendlyFire)) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, target);
        battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, damage);
    }

    public bool IsCooldownActive(long targetIncarnationId) {
        if (!IncarnationIdToHitTime.TryGetValue(targetIncarnationId, out DateTimeOffset lastHitTime)) {
            IncarnationIdToHitTime[targetIncarnationId] = DateTimeOffset.UtcNow;
            return true;
        }

        if (DateTimeOffset.UtcNow - lastHitTime < Cooldown) return true;

        IncarnationIdToHitTime.Remove(targetIncarnationId);
        return false;
    }
}