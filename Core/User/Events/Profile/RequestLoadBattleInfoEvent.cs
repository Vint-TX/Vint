using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Profile;

[ProtocolId(635890723433891050)]
public class RequestLoadBattleInfoEvent(
    LobbyProcessor lobbyProcessor
) : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        LobbyBase? lobby = lobbyProcessor.FindByBattleId(BattleId);

        if (lobby == null) return;

        await connection.Send(new BattleInfoForLabelLoadedEvent(lobby.Properties.GetValue(BattleProperty.MapInfo).Id, BattleId, lobby.Properties.GetValue(BattleProperty.BattleMode)),
            connection.UserContainer.Entity);
    }
}
