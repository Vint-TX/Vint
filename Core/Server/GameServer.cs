using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Serilog;
using Vint.Core.Battles;
using Vint.Core.ChatCommands;
using Vint.Core.Discord;
using Vint.Core.ECS.Events.Ping;
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
    public IBattleProcessor BattleProcessor { get; private set; } = null!;
    public IMatchmakingProcessor MatchmakingProcessor { get; private set; } = null!;
    public IArcadeProcessor ArcadeProcessor { get; private set; } = null!;
    public IChatCommandProcessor ChatCommandProcessor { get; private set; } = null!;
    public DiscordBot? DiscordBot { get; private set; }

    public bool IsStarted { get; private set; }
    public bool IsAccepting { get; private set; }

    public async Task Start() {
        if (IsStarted) return;

        Listener.Start();
        IsStarted = true;
        await OnStarted();

        IsAccepting = true;
        await Accept();
    }

    async Task OnStarted() {
        Logger.Information("Started");

        ChatCommandProcessor chatCommandProcessor = new();

        string? discordBotToken = Environment.GetEnvironmentVariable(DiscordDebugToken) ??
                                  Environment.GetEnvironmentVariable(DiscordProdToken);

        if (!string.IsNullOrWhiteSpace(discordBotToken))
            DiscordBot = new DiscordBot(discordBotToken, this);

        BattleProcessor = new BattleProcessor();
        MatchmakingProcessor = new MatchmakingProcessor(BattleProcessor);
        ArcadeProcessor = new ArcadeProcessor(BattleProcessor);
        ChatCommandProcessor = chatCommandProcessor;

        new Thread(MatchmakingProcessor.StartTicking) { Name = "Matchmaking ticker" }.Start();
        new Thread(ArcadeProcessor.StartTicking) { Name = "Arcade ticker" }.Start();
        new Thread(BattleProcessor.StartTicking) { Name = "Battle ticker" }.Start();
        _ = Task.Factory.StartNew(PingLoop, TaskCreationOptions.LongRunning).Catch();

        if (DiscordBot != null)
            await DiscordBot.Start();

        chatCommandProcessor.RegisterCommands();
    }

    static void OnConnected(SocketPlayerConnection connection) => connection.OnConnected();

    async Task Accept() {
        while (IsAccepting) {
            try {
                Socket socket = await Listener.AcceptSocketAsync();
                SocketPlayerConnection connection = new(this, socket, Protocol);
                OnConnected(connection);

                bool tryAdd = PlayerConnections.TryAdd(connection.Id, connection);

                if (tryAdd) continue;

                Logger.Error("Cannot add {Connection}", connection);
                connection.Kick("Internal error");
            } catch (Exception e) {
                Logger.Error(e, "");
            }
        }
    }

    public void RemovePlayer(Guid id) => PlayerConnections.Remove(id, out _);

    async Task PingLoop() {
        while (true) {
            if (!IsStarted) return;

            foreach (SocketPlayerConnection playerConnection in SocketPlayerConnections) {
                try {
                    if (!playerConnection.IsSocketConnected) {
                        playerConnection.Kick("Zombie");
                        continue;
                    }

                    playerConnection.Send(new PingEvent(DateTimeOffset.UtcNow));
                    playerConnection.PingSendTime = DateTimeOffset.UtcNow;

                    playerConnection.Tick();
                } catch (Exception e) {
                    Logger.Error(e, "Socket caught an exception while sending ping event");
                }
            }

            if (DiscordBot != null)
                await DiscordBot.SetPlayersCount(PlayerConnections.Count);

            Thread.Sleep(5000);
        }
    }
}
