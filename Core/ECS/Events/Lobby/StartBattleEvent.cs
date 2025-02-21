using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497356545125)]
public class StartBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby ||
            connection.LobbyPlayer.InRound ||
            connection.LobbyPlayer.Lobby is not CustomLobby customLobby ||
            customLobby.StateManager.CurrentState is not Awaiting ||
            customLobby.Owner != connection)
            return;

        await customLobby.Start();
    }
}
