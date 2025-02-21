using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1547616531111)]
public class ConnectToCustomLobbyEvent(
    LobbyProcessor lobbyProcessor
) : IServerEvent {
    public long LobbyId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) { // todo rework for admins
        IEntity user = connection.UserContainer.Entity;

        if (connection.InLobby) {
            await connection.Send(new EnterBattleLobbyFailedEvent(true, false), user);
            return;
        }

        if (lobbyProcessor.FindByLobbyId(LobbyId) is not CustomLobby lobby) {
            await connection.Send(new CustomLobbyNotExistsEvent(), user);
            return;
        }

        if (!lobby.IsOpened) {
            await connection.Send(new EnterBattleLobbyFailedEvent(false, false), user);
            return;
        }

        if (lobby.Players.Count >= lobby.Properties.MaxPlayers) {
            await connection.Send(new EnterBattleLobbyFailedEvent(false, true), user);
            return;
        }

        await lobby.AddPlayer(connection);
    }
}
