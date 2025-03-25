using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Effect.Type.EnergyInjection;
using Vint.Core.ECS.Components.Server.Modules.Effect.EnergyInjection;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(1128679079)]
public class EnergyInjectionModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/energyinjection";

    float ReloadEnergyPercent { get; set; }

    public override EnergyInjectionEffect GetEffect() => new(Tank, Level, ReloadEnergyPercent);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        EnergyInjectionEffect? effect = Tank
            .Effects
            .OfType<EnergyInjectionEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();

        await GetEffect()
            .Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        ReloadEnergyPercent = GetStat<ModuleEnergyInjectionEffectReloadPercentPropertyComponent>();
        await Entity.AddComponent(new EnergyInjectionModuleReloadEnergyComponent(ReloadEnergyPercent));
    }
}
