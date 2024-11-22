using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1494937115182)]
public class EnterToMatchmakingEvent : IServerEvent {
    static IEnumerable<IEntity> Modes { get; } = GlobalEntities
        .GetEntities("matchmakingModes")
        .ToList();

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (connection.InLobby) return;

        IEntity selectedMode = entities.Single();

        if (Modes.All(mode => mode.Id != selectedMode.Id)) return;

        string[]? configPathParts = selectedMode.TemplateAccessor?.ConfigPath?.Split('/');

        if (configPathParts == null) return;

        if (configPathParts[1] == "arcade") {
            if (!Enum.TryParse(configPathParts[3], true, out ArcadeModeType mode)) return;

            IArcadeProcessor arcadeProcessor = serviceProvider.GetRequiredService<IArcadeProcessor>();
            arcadeProcessor.AddPlayerToQueue(connection, mode);
        } else {
            IMatchmakingProcessor matchmakingProcessor = serviceProvider.GetRequiredService<IMatchmakingProcessor>();
            matchmakingProcessor.AddPlayerToQueue(connection);
        }

        await connection.Send(new EnteredToMatchmakingEvent(), selectedMode);
    }
}
