using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1499172594697)]
public class SwitchTeamEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby) return;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;
        if (lobbyPlayer.InRound ||
            lobbyPlayer.Lobby is not CustomLobby lobby ||
            !lobby.TeamHandler.IsTeamLobby) return;

        await lobby.TeamHandler.TrySwitchTeam(lobbyPlayer);
    }
}
