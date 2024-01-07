using Serilog;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IMatchmakingProcessor {
    public void StartTicking();

    public void AddPlayerToQueue(IPlayerConnection connection);

    public void RemovePlayerFromMatchmaking(IPlayerConnection connection, IEntity? lobby, bool selfAction);
}

public class MatchmakingProcessor(
    IBattleProcessor battleProcessor
) : IMatchmakingProcessor {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(MatchmakingProcessor));
    HashSet<IPlayerConnection> PlayerQueue { get; } = [];

    public void StartTicking() {
        try {
            while (true) {
                foreach (IPlayerConnection connection in PlayerQueue.ToArray()) {
                    if (!connection.IsOnline) {
                        PlayerQueue.Remove(connection);
                        continue;
                    }

                    battleProcessor.PutPlayerFromMatchmaking(connection);
                    PlayerQueue.Remove(connection);
                }

                Thread.Sleep(10);
            }
        } catch (Exception e) {
            Logger.Fatal(e, "Fatal error happened in matchmaking tick loop");
            throw;
        }
    }

    public void AddPlayerToQueue(IPlayerConnection connection) =>
        PlayerQueue.Add(connection);

    public void RemovePlayerFromMatchmaking(IPlayerConnection connection, IEntity? lobby, bool selfAction) {
        if (lobby != null)
            connection.Send(new ExitedFromMatchmakingEvent(selfAction), lobby);

        if (connection.InBattle) {
            BattlePlayer battlePlayer = connection.BattlePlayer!;
            Battle battle = battlePlayer.Battle;

            if (battlePlayer.InBattleAsTank)
                battle.RemovePlayer(battlePlayer);
            else
                battle.RemovePlayerFromLobby(battlePlayer);
        }

        PlayerQueue.Remove(connection);
    }
}