using Vint.Core.Battle.Effects.Templates;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Effects.Impl;

public class ForceFieldEffect : Effect {
    public ForceFieldEffect(TimeSpan duration, BattleTank tank, int level) : base(tank, level) =>
        Duration = duration;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new ForceFieldEffectTemplate().Create(Tank.Tanker, Duration);
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
