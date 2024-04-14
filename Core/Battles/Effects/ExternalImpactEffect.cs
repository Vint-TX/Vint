using System.Diagnostics.CodeAnalysis;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class ExternalImpactEffect(
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
    ExternalImpactWeaponHandler weaponHandler,
    BattleTank tank,
    int level
) : Effect(tank, level), IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; } = weaponHandler;
    
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        
        Entities.Add(new ExternalImpactEffectTemplate().Create(Tank.BattlePlayer,
            Duration,
            Battle.Properties.FriendlyFire,
            weaponHandler.Impact,
            weaponHandler.MinDamagePercent,
            weaponHandler.MaxDamageDistance,
            weaponHandler.MinDamageDistance));
        ShareAll();
        
        LastActivationTime = DateTimeOffset.UtcNow;
        Schedule(Duration, Deactivate);
    }
    
    public override void Deactivate() {
        if (!IsActive) return;
        
        Tank.Effects.TryRemove(this);
        
        UnshareAll();
        Entities.Clear();
        
        LastActivationTime = default;
    }
}