using System.Collections.Concurrent;
using Serilog;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IArcadeProcessor {
    public void StartTicking();

    public void AddPlayerToQueue(IPlayerConnection connection, ArcadeModeType mode);

    public Task RemoveArcadePlayer(IPlayerConnection connection, IEntity? lobby, bool selfAction);
}

public class ArcadeProcessor(
    IBattleProcessor battleProcessor
) : IArcadeProcessor {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(ArcadeProcessor));

    ConcurrentDictionary<IPlayerConnection, ArcadeModeType> PlayerQueue { get; } = new();

    public void StartTicking() {
        try {
            while (true) {
                foreach ((IPlayerConnection connection, ArcadeModeType mode) in PlayerQueue) {
                    if (!connection.IsOnline) {
                        PlayerQueue.TryRemove(connection, out _);
                        continue;
                    }

                    battleProcessor.PutArcadePlayer(connection, mode);
                    PlayerQueue.TryRemove(connection, out _);
                }

                Thread.Sleep(10);
            }
        } catch (Exception e) {
            Logger.Fatal(e, "Fatal error happened in arcade tick loop");
            throw;
        }
    }

    public void AddPlayerToQueue(IPlayerConnection connection, ArcadeModeType mode) =>
        PlayerQueue.AddOrUpdate(connection, mode, (_, _) => mode);

    public async Task RemoveArcadePlayer(IPlayerConnection connection, IEntity? lobby, bool selfAction) {
        if (lobby != null)
            await connection.Send(new ExitedFromMatchmakingEvent(selfAction), lobby);

        if (connection.InLobby) {
            BattlePlayer battlePlayer = connection.BattlePlayer!;
            Battle battle = battlePlayer.Battle;

            if (battlePlayer.InBattleAsTank || battlePlayer.IsSpectator)
                await battle.RemovePlayer(battlePlayer);
            else
                await battle.RemovePlayerFromLobby(battlePlayer);
        }

        PlayerQueue.TryRemove(connection, out _);
    }
}
