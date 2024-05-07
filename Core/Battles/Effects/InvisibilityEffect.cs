using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class InvisibilityEffect : Effect {
    public InvisibilityEffect(TimeSpan duration, BattleTank tank, int level) : base(tank, level) =>
        Duration = duration;

    public event Action? Deactivated;

    public override void Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new InvisibilityEffectTemplate().Create(Tank.BattlePlayer, Duration));
        ShareAll();

        Schedule(Duration, Deactivate);
    }

    public override void Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        UnshareAll();
        Entities.Clear();
        Deactivated?.Invoke();
    }
}
