using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Types.Base;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(492941809)]
public class AbsorbingArmorModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/absorbingarmor";

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();

        AbsorbingArmorEffect? effect = Tank
            .Effects
            .OfType<AbsorbingArmorEffect>()
            .SingleOrDefault();

        switch (effect) {
            case null:
                await GetEffect()
                    .Activate();

                break;

            case IExtendableEffect extendableEffect:
                await extendableEffect.Extend(Level);
                break;
        }
    }

    public override AbsorbingArmorEffect GetEffect() => new(Tank, Level);
}
