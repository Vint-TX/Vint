using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496750075382)]
public class CreateCustomBattleLobbyEvent : IServerEvent {
    public BattleProperties Properties { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.InLobby) return Task.CompletedTask;

        Battles.Battle battle = connection.Server.BattleProcessor.CreateCustomBattle(Properties, connection);
        battle.AddPlayer(connection);
        return Task.CompletedTask;
    }
}
