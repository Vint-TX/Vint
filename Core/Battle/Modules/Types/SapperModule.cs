using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Components.Server.Modules.Effect.Sapper;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(-105040547)]
public class SapperModule : TriggerBattleModule, IDamageCalculateModule {
    public override string ConfigPath => "garage/module/upgrade/properties/sapper";

    float Resistance { get; set; }

    public async Task CalculatingDamage(BattleTank source, BattleTank target, IWeaponHandler weaponHandler) {
        if (target == Tank && weaponHandler is IMineWeaponHandler)
            await Activate();
    }

    public override SapperEffect GetEffect() => new(Resistance, Tank, Level);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        SapperEffect? effect = Tank
            .Effects
            .OfType<SapperEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Resistance = GetStat<DamageResistanceEffectPropertyComponent>();
    }
}
