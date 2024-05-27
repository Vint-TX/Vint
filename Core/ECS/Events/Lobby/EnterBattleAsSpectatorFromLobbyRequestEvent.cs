using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1498554483631)]
public class EnterBattleAsSpectatorFromLobbyRequestEvent : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        Battles.Battle? battle = connection.Server.BattleProcessor.FindByBattleId(BattleId);

        if (connection.InLobby || battle == null) return;

        await battle.AddPlayer(connection, true);
    }
}
