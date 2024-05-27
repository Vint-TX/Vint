using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class TurboSpeedModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/turbospeed";

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        TurboSpeedEffect? effect = Tank.Effects.OfType<TurboSpeedEffect>().SingleOrDefault();

        switch (effect) {
            case null:
                await GetEffect().Activate();
                break;

            case IExtendableEffect extendableEffect:
                await extendableEffect.Extend(Level);
                break;
        }
    }

    public override TurboSpeedEffect GetEffect() => new(Tank, Level);
}
