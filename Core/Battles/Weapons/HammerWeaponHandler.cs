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
        SetCurrentCartridgeCount(MaximumCartridgeCount).GetAwaiter().GetResult();
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

    public override async Task OnTankEnable() {
        await base.OnTankEnable();
        await BattleTank.BattlePlayer.PlayerConnection.Send(new SetMagazineReadyEvent(), BattleEntity);
    }

    public override async Task OnTankDisable() {
        await base.OnTankDisable();
        await ResetMagazine();
    }

    public override async Task Tick() {
        await base.Tick();
        await TryReload();
    }

    public async Task SetCurrentCartridgeCount(int count) {
        if (count > MaximumCartridgeCount) return;

        CurrentCartridgeCount = count;
        await BattleEntity.ChangeComponent<MagazineStorageComponent>(component => component.CurrentCartridgeCount = CurrentCartridgeCount);

        if (CurrentCartridgeCount == 0)
            await StartReload();
    }

    public async Task StartReload() {
        await BattleEntity.RemoveComponentIfPresent<ShootableComponent>();
        await BattleEntity.AddComponentIfAbsent(new MagazineReloadStateComponent());

        ReloadEndTime = DateTimeOffset.UtcNow.AddSeconds(ReloadMagazineTimeSec);
    }

    public async Task StopReload() {
        ReloadEndTime = null;
        await BattleEntity.RemoveComponentIfPresent<MagazineReloadStateComponent>();
    }

    public async Task ResetMagazine() {
        await SetCurrentCartridgeCount(MaximumCartridgeCount);
        await StopReload();
    }

    public async Task FillMagazine() {
        await StopReload();
        await SetCurrentCartridgeCount(MaximumCartridgeCount);

        await BattleTank.BattlePlayer.PlayerConnection.Send(new SetMagazineReadyEvent(), BattleEntity);
        await BattleEntity.AddComponentIfAbsent(new ShootableComponent());
    }

    async Task TryReload() {
        if (ReloadEndTime == null || ReloadEndTime > DateTimeOffset.UtcNow) return;

        await FillMagazine();
    }
}
