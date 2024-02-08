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

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        base.Execute(connection, entities);

        Battles.Battle battle = connection.BattlePlayer!.Battle;

        if (!battle.Properties.DamageEnabled ||
            SplashTargets == null ||
            connection.BattlePlayer?.Tank?.WeaponHandler is not ThunderWeaponHandler thunder) return;

        foreach (HitTarget target in SplashTargets)
            thunder.SplashFire(target);

        using DbConnection db = new();

        db.Statistics
            .Where(stats => stats.PlayerId == connection.Player.Id)
            .Set(stats => stats.Hits, stats => stats.Hits + SplashTargets.Count)
            .Update();
    }
}