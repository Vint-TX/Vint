using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battles;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1495176527022)]
public class ExitFromMatchmakingEvent : IServerEvent {
    public bool InBattle { get; private set; }

    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return Task.CompletedTask;

        IEntity lobby = entities.Single();
        Battles.Battle battle = connection.BattlePlayer!.Battle;

        if (battle.StateManager.CurrentState is Starting)
            return Task.CompletedTask;

        switch (battle.TypeHandler) {
            case MatchmakingHandler:
                IMatchmakingProcessor matchmakingProcessor = serviceProvider.GetRequiredService<IMatchmakingProcessor>();
                matchmakingProcessor.RemovePlayerFromMatchmaking(connection, lobby, true);
                break;

            case ArcadeHandler:
                IArcadeProcessor arcadeProcessor = serviceProvider.GetRequiredService<IArcadeProcessor>();
                arcadeProcessor.RemoveArcadePlayer(connection, lobby, true);
                break;

            case CustomHandler:
                break;

            default:
                connection.Logger.ForType(GetType()).Error("Unexpected type handler: {Handler}", battle.TypeHandler.GetType().Name);
                break;
        }

        return Task.CompletedTask;
    }
}
