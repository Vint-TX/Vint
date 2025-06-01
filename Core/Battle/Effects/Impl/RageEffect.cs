using Vint.Core.Battle.Effects.Templates;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Effects.Impl;

public class RageEffect(
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new RageEffectTemplate().Create(Tank.Tanker, Duration);
        await ShareTo(Tank.Tanker);

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);
        await UnshareFrom(Tank.Tanker);

        Entity?.Dispose();
        Entity = null;
    }

    public override async Task ShareTo(BattlePlayer battlePlayer) {
        if (battlePlayer is not Tanker tanker || tanker.Tank != Tank)
            return;

        await battlePlayer.Connection.Share(Entity!);
    }

    public override async Task UnshareFrom(BattlePlayer battlePlayer) {
        if (battlePlayer is not Tanker tanker || tanker.Tank != Tank)
            return;

        await battlePlayer.Connection.Unshare(Entity!);
    }
}
