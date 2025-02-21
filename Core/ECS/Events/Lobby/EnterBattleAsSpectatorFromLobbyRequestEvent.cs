using Vint.Core.Battle.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1498554483631)]
public class EnterBattleAsSpectatorFromLobbyRequestEvent(
    LobbyProcessor lobbyProcessor
) : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.InLobby) return;

        LobbyBase? lobby = lobbyProcessor.FindByBattleId(BattleId);

        if (lobby?.StateManager.CurrentState is not Running running) return;

        await running.Round.AddSpectator(connection);
    }
}
