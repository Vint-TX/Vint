using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497349612322)]
public class AcceptInviteEvent : IServerEvent {
    [ProtocolName("lobbyId")] public long LobbyId { get; private set; }
    [ProtocolName("engineId")] public long EngineId { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.InLobby) {
            BattlePlayer battlePlayer = connection.BattlePlayer!;

            if (battlePlayer.InBattleAsTank || battlePlayer.IsSpectator)
                battlePlayer.Battle.RemovePlayer(battlePlayer);
            else
                battlePlayer.Battle.RemovePlayerFromLobby(battlePlayer);
        }

        connection.Server.BattleProcessor
            .FindByLobbyId(LobbyId)?
            .AddPlayer(connection);
    }
}