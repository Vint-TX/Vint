using Vint.Core.Battle.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(635890723433891050)]
public class RequestLoadBattleInfoEvent(
    LobbyProcessor lobbyProcessor
) : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        LobbyBase? lobby = lobbyProcessor.FindByBattleId(BattleId);

        if (lobby == null) return;

        await connection.Send(new BattleInfoForLabelLoadedEvent(lobby.Properties.ClientParams.MapId, BattleId, lobby.Properties.BattleMode),
            connection.UserContainer.Entity);
    }
}
