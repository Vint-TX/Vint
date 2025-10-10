using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Squads;

namespace Vint.Core.Battle.Lobby.Events;

[ProtocolId(1496750075382)]
public class CreateCustomBattleLobbyEvent(
    LobbyProcessor lobbyProcessor
) : IServerEvent {
    public ClientBattleParams Params { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.InLobby) return;

        Squad? squad = connection.Squad;
        if (squad != null && squad.Members.Count > Params.MaxPlayers)
            return;

        CustomLobby lobby = await lobbyProcessor.CreateCustom(Params, connection);

        if (squad != null) {
            await lobby.AddSquad(squad);
        } else
            await lobby.AddPlayer(connection);
    }
}
