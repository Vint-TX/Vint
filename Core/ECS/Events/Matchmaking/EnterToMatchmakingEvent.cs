using Vint.Core.Battle.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1494937115182)]
public class EnterToMatchmakingEvent(
    RatingMatchmakingProcessor rating,
    ArcadeMatchmakingProcessor arcade
) : IServerEvent {
    static IEnumerable<IEntity> Modes { get; } = GlobalEntities
        .GetEntities("matchmakingModes")
        .ToList();

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.InLobby) return;

        IEntity selectedMode = entities.Single();

        if (Modes.All(mode => mode.Id != selectedMode.Id)) return;

        string[]? configPathParts = selectedMode.TemplateAccessor?.ConfigPath?.Split('/');

        if (configPathParts == null) return;

        if (configPathParts[1] == "arcade") {
            if (!Enum.TryParse(configPathParts[3], true, out ArcadeModeType mode))
                return;

            await connection.Send(new EnteredToMatchmakingEvent(), selectedMode);
            await arcade.EnqueuePlayer(connection, mode);
        } else {
            await connection.Send(new EnteredToMatchmakingEvent(), selectedMode);
            await rating.EnqueuePlayer(connection);
        }
    }
}
