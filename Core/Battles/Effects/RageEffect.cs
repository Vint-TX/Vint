using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public class RageEffect(
    TimeSpan decreaseCooldownPerKill,
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new RageEffectTemplate().Create(Tank.BattlePlayer, Duration, decreaseCooldownPerKill));
        await Share(Tank.BattlePlayer);

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);
        await Unshare(Tank.BattlePlayer);

        Entities.Clear();
    }

    public override async Task Share(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;

        await battlePlayer.PlayerConnection.Share(Entities);
    }

    public override async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;

        await battlePlayer.PlayerConnection.Unshare(Entities);
    }
}
