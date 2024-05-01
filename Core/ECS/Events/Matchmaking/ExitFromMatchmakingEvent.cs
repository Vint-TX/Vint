using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1495176527022)]
public class ExitFromMatchmakingEvent : IServerEvent {
    public bool InBattle { get; private set; }

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return Task.CompletedTask;

        IEntity lobby = entities.Single();
        Battles.Battle battle = connection.BattlePlayer!.Battle;

        switch (battle.TypeHandler) {
            case MatchmakingHandler:
                connection.Server.MatchmakingProcessor.RemovePlayerFromMatchmaking(connection, lobby, true);
                break;

            case ArcadeHandler:
                connection.Server.ArcadeProcessor.RemoveArcadePlayer(connection, lobby, true);
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
