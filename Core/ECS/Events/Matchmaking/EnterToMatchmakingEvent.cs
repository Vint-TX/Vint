using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1494937115182)]
public class EnterToMatchmakingEvent : IServerEvent {
    static IEnumerable<IEntity> Modes { get; } = GlobalEntities.GetEntities("matchmakingModes").ToList();

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity selectedMode = entities.Single();

        if (Modes.All(mode => mode.Id != selectedMode.Id)) return;

        connection.Server.MatchmakingProcessor.AddPlayerToQueue(connection);
        connection.Send(new EnteredToMatchmakingEvent(), selectedMode);
    }
}