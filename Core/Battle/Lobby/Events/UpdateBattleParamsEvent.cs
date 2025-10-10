using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Lobby.State;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Events;

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
