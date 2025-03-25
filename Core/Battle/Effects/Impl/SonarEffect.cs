using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battle.Effects;

public class SonarEffect(
    BattleTank tank,
    int level
) : DurationEffect(tank, level, MarketConfigPath) {
    const string MarketConfigPath = "garage/module/upgrade/properties/sonar";

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new SonarEffectTemplate().Create(Tank.Tanker, Duration);
        await ShareTo(Tank.Tanker);

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);
        await UnshareFrom(Tank.Tanker);

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
