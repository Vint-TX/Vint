using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Serilog;
using Vint.Core.Battles;
using Vint.Core.Discord;
using Vint.Core.Quests;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game;

public class GameServer(
    IServiceProvider serviceProvider,
    DiscordBot discordBot,
    QuestManager questManager,
    IBattleProcessor battleProcessor,
    IArcadeProcessor arcadeProcessor,
    IMatchmakingProcessor matchmakingProcessor
) {
    public const ushort Port = 5050;

    public TimeSpan DeltaTime { get; private set; }
    public Dictionary<Guid, IPlayerConnection> PlayerConnections { get; } = new();

    ILogger Logger { get; } = Log.Logger.ForType(typeof(GameServer));
    TcpListener Listener { get; } = new(IPAddress.Any, Port);

    bool IsStarted { get; set; }

    public async Task Start() {
        if (IsStarted) return;

        IsStarted = true;
        Listener.Start();

        await discordBot.TryStart();

        Logger.Information("Started");
        await TickLoop();
    }

    static Task OnConnected(SocketPlayerConnection connection) => connection.OnConnected();

    async Task AcceptNewSockets() {
        for (int i = 0; i < 20 && Listener.Pending(); i++) {
            try {
                Socket socket = await Listener.AcceptSocketAsync();
                SocketPlayerConnection connection = new(serviceProvider, socket);
                await OnConnected(connection);

                if (PlayerConnections.TryAdd(connection.Id, connection)) continue;

                Logger.Error("Cannot add {Connection}", connection);
                await connection.Kick("Internal error");
            } catch (Exception e) {
                Logger.Error(e, "Exception while accepting socket");
            }
        }
    }

    public void RemovePlayer(Guid id) => PlayerConnections.Remove(id, out _);

    async Task TickPlayers() {
        await AcceptNewSockets();

        foreach (IPlayerConnection connection in PlayerConnections.Values) {
            try {
                await connection.Tick();
            } catch (Exception e) {
                Logger.Error(e, "Socket caught an exception in the players loop");
            }
        }

        await discordBot.SetPlayersCount(PlayerConnections.Count);
    }

    async Task Update() {
        matchmakingProcessor.Tick();
        arcadeProcessor.Tick();

        await battleProcessor.Tick(DeltaTime);
        await TickPlayers();

        await questManager.Tick();
    }

    async Task TickLoop() { // https://stackoverflow.com/q/78850638
        const int tps = 60;
        //const int maxTPS = 3;

        Logger.Information("HPET enabled: {Value}", Stopwatch.IsHighResolution);

        TimeSpan targetDeltaTime = TimeSpan.FromSeconds(1d / tps);
        //TimeSpan maximumDeltaTime = TimeSpan.FromSeconds(1d / maxTPS);

        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan lastTick = stopwatch.Elapsed;

        while (IsStarted) {
            TimeSpan currentTick = stopwatch.Elapsed;
            //DeltaTime = TimeSpanUtils.Min(currentTick - lastTick, maximumDeltaTime);
            DeltaTime = currentTick - lastTick;
            lastTick = currentTick;

            try {
                await Update();
            } catch (Exception e) {
                Logger.Error(e, "Caught an exception in game loop");
            }

            TimeSpan freeTime = targetDeltaTime - (stopwatch.Elapsed - currentTick);

            if (freeTime > TimeSpan.Zero)
                await Task.Delay(freeTime);
        }
    }
}
