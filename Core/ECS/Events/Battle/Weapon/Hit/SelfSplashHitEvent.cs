using Vint.Core.Battles.Player;
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
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if (!battlePlayer.InBattleAsTank) return;

        IEntity weapon = entities.Single();
        Battles.Battle battle = battlePlayer.Battle;

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(RemoteEvent, weapon);

        // todo
    }
}