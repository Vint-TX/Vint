using System.Diagnostics;
using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class HammerWeaponHandler : WeaponHandler {
    public HammerWeaponHandler(BattleTank battleTank) : base(battleTank) {
        MagazineWeaponComponent magazineWeaponComponent = ConfigManager.GetComponent<MagazineWeaponComponent>(MarketConfigPath);

        PelletCount = BattleEntity.GetComponent<HammerPelletConeComponent>().PelletCount;
        DamagePerPellet = ConfigManager.GetComponent<DamagePerPelletPropertyComponent>(MarketConfigPath).FinalValue;
        ReloadMagazineTimeSec = magazineWeaponComponent.ReloadMagazineTimePerSec;
        MaximumCartridgeCount = magazineWeaponComponent.MaxCartridgeCount;

        BattleEntity.AddComponent(new MagazineStorageComponent(MaximumCartridgeCount));
        SetCurrentCartridgeCount(MaximumCartridgeCount);
    }

    public float ReloadMagazineTimeSec { get; }
    public float DamagePerPellet { get; }
    public int PelletCount { get; }

    public int MaximumCartridgeCount { get; }
    public int CurrentCartridgeCount { get; private set; }

    DateTime? ReloadEndTime { get; set; }

    public override int MaxHitTargets => PelletCount;

    public override void Fire(HitTarget target, int targetIndex) => throw new UnreachableException();

    public void Fire(List<HitTarget> hitTargets) {
        Battle battle = BattleTank.Battle;
        List<BattleTank> tanks = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .ToList();

        Dictionary<BattleTank, CalculatedDamage> tankToDamage = new();

        for (int i = 0; i < hitTargets.Count; i++) {
            HitTarget hitTarget = hitTargets[i];
            BattleTank targetTank = tanks.Single(battleTank => battleTank.Incarnation == hitTarget.IncarnationEntity);

            bool isEnemy = BattleTank.IsEnemy(targetTank);

            // ReSharper disable once ArrangeRedundantParentheses
            if (targetTank.StateManager.CurrentState is not Active ||
                (!isEnemy && !battle.Properties.FriendlyFire)) continue;

            CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, hitTarget, i);

            if (tankToDamage.TryAdd(targetTank, damage)) continue;

            CalculatedDamage calculatedDamage = tankToDamage[targetTank];

            calculatedDamage = calculatedDamage with {
                HitPoint = damage.IsBackHit ? damage.HitPoint : calculatedDamage.HitPoint,
                Value = damage.Value + calculatedDamage.Value,
                IsBackHit = damage.IsBackHit || calculatedDamage.IsBackHit
            };

            tankToDamage[targetTank] = calculatedDamage;
        }

        foreach ((BattleTank targetTank, CalculatedDamage damage) in tankToDamage)
            battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, damage);
    }

    public override void OnTankDisable() {
        base.OnTankDisable();
        ResetMagazine();
    }

    public override void Tick() {
        base.Tick();
        TryReload();
    }

    public void SetCurrentCartridgeCount(int count) {
        if (count > MaximumCartridgeCount) return;

        CurrentCartridgeCount = count;
        BattleEntity.ChangeComponent<MagazineStorageComponent>(component => component.CurrentCartridgeCount = CurrentCartridgeCount);

        if (CurrentCartridgeCount == 0)
            StartReload();
    }

    public void StartReload() {
        BattleEntity.RemoveComponentIfPresent<ShootableComponent>();
        BattleEntity.AddComponentIfAbsent(new MagazineReloadStateComponent());

        ReloadEndTime = DateTime.UtcNow.AddSeconds(ReloadMagazineTimeSec);
    }

    public void StopReload() {
        ReloadEndTime = null;
        BattleEntity.RemoveComponentIfPresent<MagazineReloadStateComponent>();
    }

    public void ResetMagazine() {
        SetCurrentCartridgeCount(MaximumCartridgeCount);
        StopReload();
    }

    public void FillMagazine() { // todo modules
        StopReload();
        SetCurrentCartridgeCount(MaximumCartridgeCount);

        BattleTank.BattlePlayer.PlayerConnection.Send(new SetMagazineReadyEvent(), BattleEntity);
        BattleEntity.AddComponentIfAbsent(new ShootableComponent());
    }

    void TryReload() {
        if (ReloadEndTime == null || ReloadEndTime > DateTime.UtcNow) return;

        FillMagazine();
    }
}