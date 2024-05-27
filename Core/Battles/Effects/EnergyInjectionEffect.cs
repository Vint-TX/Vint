using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class EnergyInjectionEffect(
    BattleTank tank,
    int level,
    float reloadPercent
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new EnergyInjectionEffectTemplate().Create(Tank.BattlePlayer, Duration, reloadPercent));
        await ShareAll();

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareAll();
        Entities.Clear();
    }
}
