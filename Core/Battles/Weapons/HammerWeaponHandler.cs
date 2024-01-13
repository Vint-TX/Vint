using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon;

namespace Vint.Core.Battles.Weapons;

public class HammerWeaponHandler : WeaponHandler {
    public HammerWeaponHandler(BattleTank battleTank) : base(battleTank) {
        MagazineWeaponComponent magazineWeaponComponent = ConfigManager.GetComponent<MagazineWeaponComponent>(MarketConfigPath);

        DamagePerPellet = ConfigManager.GetComponent<Damage.DamagePerPelletPropertyComponent>(MarketConfigPath).FinalValue;
        ReloadMagazineTimeSec = magazineWeaponComponent.ReloadMagazineTimePerSec;
        MaximumCartridgeCount = magazineWeaponComponent.MaxCartridgeCount;

        BattleEntity.AddComponent(new MagazineStorageComponent(MaximumCartridgeCount));
        SetCurrentCartridgeCount(MaximumCartridgeCount);
    }

    public float ReloadMagazineTimeSec { get; }
    public float DamagePerPellet { get; }
    
    public int MaximumCartridgeCount { get; }
    public int CurrentCartridgeCount { get; private set; }

    DateTime? ReloadEndTime { get; set; }
    
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
        if (BattleEntity.HasComponent<ShootableComponent>())
            BattleEntity.RemoveComponent<ShootableComponent>();
        
        if (!BattleEntity.HasComponent<MagazineReloadStateComponent>())
            BattleEntity.AddComponent(new MagazineReloadStateComponent());
        
        ReloadEndTime = DateTime.UtcNow.AddSeconds(ReloadMagazineTimeSec);
    }

    public void StopReload() {
        ReloadEndTime = null;
        
        if (BattleEntity.HasComponent<MagazineReloadStateComponent>())
            BattleEntity.RemoveComponent<MagazineReloadStateComponent>();
    }

    public void ResetMagazine() {
        SetCurrentCartridgeCount(MaximumCartridgeCount);
        StopReload();
    }

    public void FillMagazine() { // todo modules
        StopReload();
        SetCurrentCartridgeCount(MaximumCartridgeCount);
        
        if (!BattleEntity.HasComponent<ShootableComponent>()) 
            BattleEntity.AddComponent(new ShootableComponent());
        
        BattleTank.BattlePlayer.PlayerConnection.Send(new SetMagazineReadyEvent(), BattleEntity);
    }

    void TryReload() {
        if (ReloadEndTime == null || ReloadEndTime > DateTime.UtcNow) return;
        
        FillMagazine();
    }
}