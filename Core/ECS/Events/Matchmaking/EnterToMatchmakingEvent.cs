using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1494937115182)]
public class EnterToMatchmakingEvent : IServerEvent {
    static IEnumerable<IEntity> Modes { get; } = GlobalEntities.GetEntities("matchmakingModes").ToList();

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.InLobby) return Task.CompletedTask;

        IEntity selectedMode = entities.Single();

        if (Modes.All(mode => mode.Id != selectedMode.Id)) return Task.CompletedTask;

        string[]? configPathParts = selectedMode.TemplateAccessor?.ConfigPath?.Split('/');

        if (configPathParts == null) return Task.CompletedTask;

        if (configPathParts[1] == "arcade") {
            if (!Enum.TryParse(configPathParts[3], true, out ArcadeModeType mode)) return Task.CompletedTask;

            connection.Server.ArcadeProcessor.AddPlayerToQueue(connection, mode);
        } else {
            connection.Server.MatchmakingProcessor.AddPlayerToQueue(connection);
        }

        connection.Send(new EnteredToMatchmakingEvent(), selectedMode);
        return Task.CompletedTask;
    }
}
