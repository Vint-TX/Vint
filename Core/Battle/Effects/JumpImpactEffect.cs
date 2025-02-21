using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battle.Effects;

public class JumpImpactEffect(
    BattleTank tank,
    int level,
    float force
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new JumpEffectTemplate().Create(Tank.Tanker, Duration, force);
        await ShareToAllPlayers();

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }
}
