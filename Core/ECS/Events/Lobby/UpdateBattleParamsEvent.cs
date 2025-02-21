using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497614958932)]
public class UpdateBattleParamsEvent : IServerEvent {
    public ClientBattleParams Params { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby ||
            connection.LobbyPlayer.Lobby is not CustomLobby lobby ||
            lobby.Owner != connection ||
            lobby.StateManager.CurrentState is not Awaiting) return;

        await lobby.UpdateClientProperties(Params);
    }
}
