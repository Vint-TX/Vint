using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1495176527022)]
public class ExitFromMatchmakingEvent : IServerEvent {
    public bool InBattle { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity lobby = entities.Single();

        connection.Server.MatchmakingProcessor.RemovePlayerFromMatchmaking(connection, lobby, true);
    }
}