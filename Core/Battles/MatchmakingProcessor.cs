using Serilog;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IMatchmakingProcessor {
    public void StartTicking();

    public void AddPlayerToQueue(IPlayerConnection connection);

    public void RemovePlayerFromQueue(IPlayerConnection connection);
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
                        RemovePlayerFromQueue(connection);
                        continue;
                    }

                    battleProcessor.PutPlayerFromMatchmaking(connection);
                    RemovePlayerFromQueue(connection);
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

    public void RemovePlayerFromQueue(IPlayerConnection connection) =>
        PlayerQueue.Remove(connection);
}