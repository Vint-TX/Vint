using System.Collections.Frozen;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Battle.Lobby;
using Vint.Core.Discord;
using Vint.Core.Quests;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game;

public class GameServer {
    public const ushort Port = 5050;
    int _lastClientId = -1;

    public GameServer(IServiceProvider serviceProvider, DiscordBot discordBot, QuestManager questManager, LobbyProcessor lobbyProcessor) {
        ServiceProvider = serviceProvider;
        DiscordBot = discordBot;
        QuestManager = questManager;
        LobbyProcessor = lobbyProcessor;

        Looper = new Looper(Update, 60);
    }

    public Looper Looper { get; }
    public Dictionary<int, IPlayerConnection> PlayerConnections { get; } = new();

    ILogger Logger { get; } = Log.Logger.ForType<GameServer>();
    TcpListener Listener { get; } = new(IPAddress.Any, Port);

    IServiceProvider ServiceProvider { get; }
    DiscordBot DiscordBot { get; }
    QuestManager QuestManager { get; }
    LobbyProcessor LobbyProcessor { get; }

    bool IsStarted { get; set; }

    public async Task Start() {
        if (IsStarted) return;

        IsStarted = true;
        Listener.Start();

        await DiscordBot.TryStart();

        Logger.Information("Started");
        await Looper.RunAsync();
    }

    public IPlayerConnection? FindConnection(long id) =>
        PlayerConnections.Values.FirstOrDefault(connection => connection.IsLoggedIn &&
                                                              connection.UserContainer.Id == id);

    public IEnumerable<IPlayerConnection> FindConnections(params FrozenSet<long> ids) =>
        PlayerConnections.Values.Where(connection => connection.IsLoggedIn &&
                                                     ids.Contains(connection.UserContainer.Id));

    public void RemovePlayer(int id) => PlayerConnections.Remove(id, out _);

    async Task Update(TimeSpan deltaTime) {
        await LobbyProcessor.Tick(deltaTime);
        await TickPlayers();
        await QuestManager.Tick();
    }

    async Task TickPlayers() {
        await AcceptNewSockets();

        foreach (IPlayerConnection connection in PlayerConnections.Values) {
            try {
                await connection.Tick();
            } catch (Exception e) {
                Logger.Error(e, "Socket caught an exception in the players loop");
            }
        }

        await DiscordBot.SetPlayersCount(PlayerConnections.Count);
    }

    async Task AcceptNewSockets() {
        for (int i = 0; i < 20 && Listener.Pending(); i++) {
            try {
                int id = Interlocked.Increment(ref _lastClientId);
                IServiceScope serviceScope = ServiceProvider.CreateAsyncScope();

                Socket socket = await Listener.AcceptSocketAsync();
                SocketPlayerConnection connection = new(id, serviceScope, socket);

                PlayerConnections[id] = connection;
                await connection.OnConnected();
            } catch (Exception e) {
                Logger.Error(e, "Exception while accepting socket");
            }
        }
    }
}
