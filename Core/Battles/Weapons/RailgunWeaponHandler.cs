using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class RailgunWeaponHandler : DiscreteTankWeaponHandler {
    public RailgunWeaponHandler(BattleTank battleTank) : base(battleTank) =>
        DamageWeakeningByTargetPercent = ConfigManager.GetComponent<DamageWeakeningByTargetPropertyComponent>(MarketConfigPath).FinalValue / 100;

    public float DamageWeakeningByTargetPercent { get; }
    public override int MaxHitTargets => BattleTank.Battle.Players.Count;

    public override async Task Fire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }
}
