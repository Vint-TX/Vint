using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(196833391289212110)]
public class SelfSplashHitEvent : SelfHitEvent {
    public List<HitTarget>? SplashTargets { get; private set; }

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if (!battlePlayer.InBattleAsTank) return;

        IEntity weapon = entities.Single();
        Battles.Battle battle = battlePlayer.Battle;

        RemoteSplashHitEvent serverEvent = new() {
            Targets = Targets,
            StaticHit = StaticHit,
            ShotId = ShotId,
            ClientTime = ClientTime,
            SplashTargets = SplashTargets
        };

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(serverEvent, weapon);

        // todo
    }
}