using System.Collections.Concurrent;
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
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Server.Game;

public class GameServer(
    IServiceProvider serviceProvider,
    QuestManager questManager,
    LobbyProcessor lobbyProcessor
) : BackgroundService {
    public const ushort Port = 5050;
    int _lastConnectionId = -1;

    public TimeSpan DeltaTime { get; private set; }
    public IEnumerable<IPlayerConnection> Connections => PlayerConnections.Values.Where(conn => !conn.IsBot);
    public ICollection<IPlayerConnection> ConnectionsWithBots => PlayerConnections.Values;

    ConcurrentDictionary<int, IPlayerConnection> PlayerConnections { get; } = new();

    ILogger Logger { get; } = Log.Logger.ForType<GameServer>();
    TcpListener Listener { get; } = new(IPAddress.Any, Port);

    int LastPlayersCount { get; set; }

    public IPlayerConnection? FindConnection(long id) =>
        Connections.FirstOrDefault(connection => connection.IsLoggedIn &&
                                                 connection.UserContainer.Id == id);

    public IEnumerable<IPlayerConnection> FindConnections(params FrozenSet<long> ids) =>
        Connections.Where(connection => connection.IsLoggedIn &&
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
                int id = GenerateConnectionId();
                IServiceScope serviceScope = serviceProvider.CreateAsyncScope();

                Socket socket = await Listener.AcceptSocketAsync(cancellationToken);
                SocketPlayerConnection connection = new(id, serviceScope, socket);

                await PlayerConnected(connection);
            } catch (Exception e) {
                Logger.Error(e, "Exception while accepting socket");
            }
        }
    }

    public async Task PlayerConnected(IPlayerConnection connection) {
        AddPlayer(connection.Id, connection);
        await connection.OnConnected();
    }

    void AddPlayer(int id, IPlayerConnection connection) {
        if (PlayerConnections.TryAdd(id, connection))
            Logger.Information("Added player connection with id {Id}", id);
        else {
            Logger.Error("Player connection with id {Id} already exists", id);
            throw new InvalidOperationException($"Player connection with id {id} already exists");
        }
    }

    public void RemovePlayer(int id) => PlayerConnections.TryRemove(id, out _);

    public int GenerateConnectionId() => Interlocked.Increment(ref _lastConnectionId);

    async Task TickPlayers(CancellationToken cancellationToken) {
        await AcceptNewSockets(cancellationToken);

        foreach (IPlayerConnection connection in ConnectionsWithBots) {
            try {
                await connection.Tick(cancellationToken);
            } catch (Exception e) {
                Logger.Error(e, "Socket caught an exception in the players loop");
            }
        }

        int currentPlayersCount = Connections.Count();
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
