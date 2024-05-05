using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class HammerWeaponHandler : TankWeaponHandler {
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

    DateTimeOffset? ReloadEndTime { get; set; }

    public override int MaxHitTargets => PelletCount;

    public override Task Fire(HitTarget target, int targetIndex) => throw new NotSupportedException();

    public async Task Fire(List<HitTarget> hitTargets) {
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
            if (targetTank.StateManager.CurrentState is not Active || !isEnemy) continue;

            CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, hitTarget, i);

            if (tankToDamage.TryAdd(targetTank, damage)) continue;

            CalculatedDamage calculatedDamage = tankToDamage[targetTank];

            calculatedDamage = calculatedDamage with {
                HitPoint = damage.IsSpecial ? damage.HitPoint : calculatedDamage.HitPoint,
                Value = damage.Value + calculatedDamage.Value,
                IsSpecial = damage.IsSpecial || calculatedDamage.IsSpecial
            };

            tankToDamage[targetTank] = calculatedDamage;
        }

        foreach ((BattleTank targetTank, CalculatedDamage damage) in tankToDamage)
            await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public override void OnTankEnable() {
        base.OnTankEnable();
        BattleTank.BattlePlayer.PlayerConnection.Send(new SetMagazineReadyEvent(), BattleEntity);
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

        ReloadEndTime = DateTimeOffset.UtcNow.AddSeconds(ReloadMagazineTimeSec);
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
        if (ReloadEndTime == null || ReloadEndTime > DateTimeOffset.UtcNow) return;

        FillMagazine();
    }
}
