using ConcurrentCollections;
using Serilog;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IMatchmakingProcessor {
    void Tick();

    void AddPlayerToQueue(IPlayerConnection connection);

    Task RemovePlayerFromMatchmaking(IPlayerConnection connection, IEntity? lobby, bool selfAction);
}

public class MatchmakingProcessor(
    IBattleProcessor battleProcessor
) : IMatchmakingProcessor {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(MatchmakingProcessor));
    ConcurrentHashSet<IPlayerConnection> PlayerQueue { get; } = [];

    public void Tick() {
        foreach (IPlayerConnection connection in PlayerQueue) {
            try {
                if (!connection.IsOnline) {
                    PlayerQueue.TryRemove(connection);
                    continue;
                }

                battleProcessor.PutPlayerFromMatchmaking(connection);
                PlayerQueue.TryRemove(connection);
            } catch (Exception e) {
                Logger.Error(e, "Caught an exception in matchmaking loop");
            }
        }
    }

    public void AddPlayerToQueue(IPlayerConnection connection) =>
        PlayerQueue.Add(connection);

    public async Task RemovePlayerFromMatchmaking(IPlayerConnection connection, IEntity? lobby, bool selfAction) {
        if (lobby != null)
            await connection.Send(new ExitedFromMatchmakingEvent(selfAction), lobby);

        if (connection.InLobby) {
            BattlePlayer battlePlayer = connection.BattlePlayer!;
            Battle battle = battlePlayer.Battle;

            if (battlePlayer.InBattleAsTank ||
                battlePlayer.IsSpectator)
                await battle.RemovePlayer(battlePlayer);
            else
                await battle.RemovePlayerFromLobby(battlePlayer);
        }

        PlayerQueue.TryRemove(connection);
    }
}
