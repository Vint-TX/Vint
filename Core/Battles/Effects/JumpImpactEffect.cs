using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class JumpImpactEffect(
    BattleTank tank,
    int level,
    float force
) : Effect(tank, level) {
    public override void Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new JumpEffectTemplate().Create(Tank.BattlePlayer, Duration, force));
        ShareAll();

        Schedule(Duration, Deactivate);
    }

    public override void Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        UnshareAll();
        Entities.Clear();
    }
}
