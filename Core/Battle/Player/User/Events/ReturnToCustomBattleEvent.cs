using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Running = Vint.Core.Battle.Lobby.Running;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(1498743823980)]
public class ReturnToCustomBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        LobbyPlayer? lobbyPlayer = connection.LobbyPlayer;
        CustomLobby? customLobby = lobbyPlayer?.Lobby as CustomLobby;
        Round? round = lobbyPlayer?.Round;

        if (lobbyPlayer is not { InRound: false } || // lobby player is null or already in round
            customLobby?.StateManager.CurrentState is not Running || // lobby is not custom or not running
            round == null) // round is not started
            return;

        await round.AddTanker(lobbyPlayer);
    }
}
