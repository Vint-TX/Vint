using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class IncreasedDamageModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/increaseddamage";

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        IncreasedDamageEffect? effect = Tank.Effects.OfType<IncreasedDamageEffect>().SingleOrDefault();

        switch (effect) {
            case null:
                await GetEffect().Activate();
                break;

            case IExtendableEffect extendableEffect:
                await extendableEffect.Extend(Level);
                break;
        }
    }

    public override IncreasedDamageEffect GetEffect() => new(Tank, Level);
}
