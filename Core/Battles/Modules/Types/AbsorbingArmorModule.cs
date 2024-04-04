using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class AbsorbingArmorModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/absorbingarmor";

    public override void Activate() {
        if (!CanBeActivated) return;

        base.Activate();
        AbsorbingArmorEffect? effect = Tank.Effects.OfType<AbsorbingArmorEffect>().SingleOrDefault();

        switch (effect) {
            case null:
                GetEffect().Activate();
                break;

            case IExtendableEffect extendableEffect:
                extendableEffect.Extend(Level);
                break;
        }
    }

    public override AbsorbingArmorEffect GetEffect() => new(Tank, Level);
}