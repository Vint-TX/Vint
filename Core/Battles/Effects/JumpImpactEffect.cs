using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class JumpImpactEffect(
    BattleTank tank,
    int level,
    float force
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new JumpEffectTemplate().Create(Tank.BattlePlayer, Duration, force));
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
