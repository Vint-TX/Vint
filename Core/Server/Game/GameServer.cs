using System.Collections.Frozen;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Vint.Core.Battle.Lobby;
using Vint.Core.Logging;
using Vint.Core.Quests;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;

namespace Vint.Core.Server.Game;

public class GameServer(
    IServiceProvider serviceProvider,
    QuestManager questManager,
    LobbyProcessor lobbyProcessor
) : BackgroundService {
    public const ushort Port = 5050;
    int _lastClientId = -1;

    public TimeSpan DeltaTime { get; private set; }
    public Dictionary<int, IPlayerConnection> PlayerConnections { get; } = new();

    ILogger Logger { get; } = Log.Logger.ForType<GameServer>();
    TcpListener Listener { get; } = new(IPAddress.Any, Port);

    int LastPlayersCount { get; set; }

    public IPlayerConnection? FindConnection(long id) =>
        PlayerConnections.Values.FirstOrDefault(connection => connection.IsLoggedIn &&
                                                              connection.UserContainer.Id == id);

    public IEnumerable<IPlayerConnection> FindConnections(params FrozenSet<long> ids) =>
        PlayerConnections.Values.Where(connection => connection.IsLoggedIn &&
                                                     ids.Contains(connection.UserContainer.Id));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        Listener.Start();

        Logger.Information("Started");
        await TickLoop(stoppingToken);
        Logger.Information("Stopped");
    }

    async Task AcceptNewSockets(CancellationToken cancellationToken) {
        for (int i = 0; i < 20 && Listener.Pending(); i++) {
            try {
                int id = Interlocked.Increment(ref _lastClientId);
                IServiceScope serviceScope = serviceProvider.CreateAsyncScope();

                Socket socket = await Listener.AcceptSocketAsync(cancellationToken);
                SocketPlayerConnection connection = new(id, serviceScope, socket);

                PlayerConnections[id] = connection;
                await connection.OnConnected();
            } catch (Exception e) {
                Logger.Error(e, "Exception while accepting socket");
            }
        }
    }

    public void RemovePlayer(int id) => PlayerConnections.Remove(id, out _);

    async Task TickPlayers(CancellationToken cancellationToken) {
        await AcceptNewSockets(cancellationToken);

        foreach (IPlayerConnection connection in PlayerConnections.Values) {
            try {
                await connection.Tick(cancellationToken);
            } catch (Exception e) {
                Logger.Error(e, "Socket caught an exception in the players loop");
            }
        }

        int currentPlayersCount = PlayerConnections.Count;
        if (currentPlayersCount == LastPlayersCount) return;

        await serviceProvider.GetRequiredService<ApiServer>().PlayersCountChanged(currentPlayersCount);
        LastPlayersCount = currentPlayersCount;
    }

    async Task Update(CancellationToken cancellationToken) {
        await lobbyProcessor.Tick(DeltaTime, cancellationToken);
        await TickPlayers(cancellationToken);

        await questManager.Tick(cancellationToken);
    }

    async Task TickLoop(CancellationToken cancellationToken) { // https://stackoverflow.com/q/78850638
        const int tps = 60;
        //const int maxTPS = 3;

        //Logger.Information("HPET enabled: {Value}", Stopwatch.IsHighResolution);

        TimeSpan targetDeltaTime = TimeSpan.FromSeconds(1d / tps);
        //TimeSpan maximumDeltaTime = TimeSpan.FromSeconds(1d / maxTPS);

        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan lastTick = stopwatch.Elapsed;

        while (!cancellationToken.IsCancellationRequested) {
            TimeSpan currentTick = stopwatch.Elapsed;
            //DeltaTime = TimeSpanUtils.Min(currentTick - lastTick, maximumDeltaTime);
            DeltaTime = currentTick - lastTick;
            lastTick = currentTick;

            try {
                await Update(cancellationToken);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception e) {
                Logger.Error(e, "Caught an exception in game loop");
            }

            TimeSpan freeTime = targetDeltaTime - (stopwatch.Elapsed - currentTick);

            try {
                if (freeTime > TimeSpan.Zero)
                    await Task.Delay(freeTime, cancellationToken);
            } catch (OperationCanceledException) {
                // Ignore
            }
        }
    }

    public override void Dispose() {
        base.Dispose();

        Listener.Dispose();
        GC.SuppressFinalize(this);
    }
}
