using LinqToDB;
using Vint.Core.Battles.Weapons;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(196833391289212110)]
public class SelfSplashHitEvent : SelfHitEvent {
    public List<HitTarget>? SplashTargets { get; private set; }

    [ProtocolIgnore] protected override RemoteSplashHitEvent RemoteEvent => new() {
        SplashTargets = SplashTargets,
        ClientTime = ClientTime,
        StaticHit = StaticHit,
        ShotId = ShotId,
        Targets = Targets
    };

    public override async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        await base.Execute(connection, entities);

        if (!IsProceeded) return;

        Battles.Battle battle = connection.BattlePlayer!.Battle;

        if (!battle.Properties.DamageEnabled ||
            SplashTargets == null ||
            WeaponHandler is not ISplashWeaponHandler splashHandler) return;

        for (int i = 0; i < SplashTargets.Count; i++) {
            HitTarget target = SplashTargets[i];
            await splashHandler.SplashFire(target, i);
        }

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == connection.Player.Id)
            .Set(stats => stats.Hits, stats => stats.Hits + SplashTargets.Count)
            .UpdateAsync();
    }
}
