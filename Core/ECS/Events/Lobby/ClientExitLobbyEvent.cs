using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496753144455)]
public class ClientExitLobbyEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (battlePlayer.InBattleAsTank || battlePlayer.IsSpectator)
            battle.RemovePlayer(battlePlayer);
        else
            battle.RemovePlayerFromLobby(battlePlayer);
    }
}