using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot;

[ProtocolId(5440037691022467911)]
public class SelfShotEvent : ShotEvent, IServerEvent {
    [ProtocolIgnore] protected virtual RemoteShotEvent RemoteEvent => new() {
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public virtual void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return;

        IEntity tank = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(RemoteEvent, tank);

        // todo
    }
}