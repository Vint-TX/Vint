using Vint.Core.Battles.Player;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class RageEffect(
    TimeSpan decreaseCooldownPerKill,
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new RageEffectTemplate().Create(Tank.BattlePlayer, Duration, decreaseCooldownPerKill);
        await Share(Tank.BattlePlayer);

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);
        await Unshare(Tank.BattlePlayer);

        Entity = null;
    }

    public override async Task Share(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;

        await battlePlayer.PlayerConnection.Share(Entity!);
    }

    public override async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;

        await battlePlayer.PlayerConnection.Unshare(Entity!);
    }
}
