using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class ForceFieldEffect : Effect {
    public ForceFieldEffect(TimeSpan duration, BattleTank tank, int level) : base(tank, level) =>
        Duration = duration;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new ForceFieldEffectTemplate().Create(Tank.BattlePlayer, Duration));
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
