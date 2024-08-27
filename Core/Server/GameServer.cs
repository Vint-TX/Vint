using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Serilog;
using Vint.Core.Battles;
using Vint.Core.ChatCommands;
using Vint.Core.Discord;
using Vint.Core.Quests;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public class GameServer(
    IPAddress host,
    ushort port
) {
    const string DiscordDebugToken = "VINT_DISCORD_BOT_DEBUG_TOKEN", DiscordProdToken = "VINT_DISCORD_BOT_PROD_TOKEN";

    ILogger Logger { get; } = Log.Logger.ForType(typeof(GameServer));
    Protocol.Protocol Protocol { get; } = new();
    TcpListener Listener { get; } = new(host, port);
    IEnumerable<SocketPlayerConnection> SocketPlayerConnections => PlayerConnections.Values.Cast<SocketPlayerConnection>();

    public ConcurrentDictionary<Guid, IPlayerConnection> PlayerConnections { get; } = new();
    public QuestManager QuestManager { get; private set; } = null!;
    public IBattleProcessor BattleProcessor { get; private set; } = null!;
    public IMatchmakingProcessor MatchmakingProcessor { get; private set; } = null!;
    public IArcadeProcessor ArcadeProcessor { get; private set; } = null!;
    public IChatCommandProcessor ChatCommandProcessor { get; private set; } = null!;
    public DiscordBot? DiscordBot { get; private set; }

    bool IsStarted { get; set; }
    bool IsAccepting { get; set; }
    public static TimeSpan DeltaTime { get; private set; }

    public async Task Start() {
        if (IsStarted) return;

        Listener.Start();
        IsStarted = true;
        OnStarted();

        IsAccepting = true;
        await Accept();
    }

    void OnStarted() {
        Logger.Information("Started");

        ChatCommandProcessor chatCommandProcessor = new();

        string? discordBotToken = Environment.GetEnvironmentVariable(DiscordDebugToken) ??
                                  Environment.GetEnvironmentVariable(DiscordProdToken);

        if (!string.IsNullOrWhiteSpace(discordBotToken))
            DiscordBot = new DiscordBot(discordBotToken, this);

        QuestManager = new QuestManager(this);
        BattleProcessor = new BattleProcessor();
        MatchmakingProcessor = new MatchmakingProcessor(BattleProcessor);
        ArcadeProcessor = new ArcadeProcessor(BattleProcessor);
        ChatCommandProcessor = chatCommandProcessor;

        Extensions.RunTaskInBackground(TickLoop, OnException, true);

        if (DiscordBot != null)
            Extensions.RunTaskInBackground(DiscordBot.Start, OnException, true);

        chatCommandProcessor.RegisterCommands();
    }

    void OnException(Exception e) {
        Logger.Fatal(e, "");
        Environment.Exit(e.HResult);
    }

    static Task OnConnected(SocketPlayerConnection connection) => connection.OnConnected();

    async Task Accept() {
        while (IsAccepting) {
            try {
                Socket socket = await Listener.AcceptSocketAsync();
                SocketPlayerConnection connection = new(this, socket, Protocol);
                await OnConnected(connection);

                if (PlayerConnections.TryAdd(connection.Id, connection)) continue;

                Logger.Error("Cannot add {Connection}", connection);
                await connection.Kick("Internal error");
            } catch (Exception e) {
                Logger.Error(e, "");
            }
        }
    }

    public void RemovePlayer(Guid id) => PlayerConnections.Remove(id, out _);

    async Task TickPlayers() {
        foreach (SocketPlayerConnection playerConnection in SocketPlayerConnections) {
            try {
                await playerConnection.Tick();
            } catch (Exception e) {
                Logger.Error(e, "Socket caught an exception in the players loop");
            }
        }

        if (DiscordBot != null)
            await DiscordBot.SetPlayersCount(PlayerConnections.Count);
    }

    async Task Update() {
        MatchmakingProcessor.Tick();
        ArcadeProcessor.Tick();

        await BattleProcessor.Tick();
        await TickPlayers();

        await QuestManager.Tick();
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
