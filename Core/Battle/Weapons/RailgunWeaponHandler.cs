using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Damage;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public class RailgunWeaponHandler : DiscreteTankWeaponHandler {
    public RailgunWeaponHandler(BattleTank battleTank) : base(battleTank) =>
        DamageWeakeningByTargetPercent = ConfigManager.GetComponent<DamageWeakeningByTargetPropertyComponent>(MarketConfigPath).FinalValue / 100;

    public float DamageWeakeningByTargetPercent { get; }
    public override int MaxHitTargets => BattleTank.Round.Tankers.Count();

    public override async Task Fire(HitTarget target, int targetIndex) {
        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }
}
