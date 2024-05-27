using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public class SonarEffect(
    BattleTank tank,
    int level
) : DurationEffect(tank, level, MarketConfigPath) {
    const string MarketConfigPath = "garage/module/upgrade/properties/sonar";

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new SonarEffectTemplate().Create(Tank.BattlePlayer, Duration));
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
