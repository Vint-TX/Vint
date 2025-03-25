using Vint.Core.Battle.Effects.Templates;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Effects.Impl;

public class LifeStealEffect(
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new LifestealEffectTemplate().Create(Tank.Tanker, Duration);
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
