using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Modules.Impl.Base;

namespace Vint.Core.Battle.Modules.Impl;

[ModuleId(-1318192334)]
public class SonarModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/sonar";

    public override SonarEffect GetEffect() => new(Tank, Level);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        SonarEffect? effect = Tank
            .Effects
            .OfType<SonarEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();

        await GetEffect()
            .Activate();
    }
}
