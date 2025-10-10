using System.Diagnostics.CodeAnalysis;
using ConcurrentCollections;
using Microsoft.Extensions.DependencyInjection;
using Redzen.Numerics.Distributions.Double;
using Redzen.Random;
using Serilog;
using Serilog.Events;
using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Player.Score.Events;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.RestorePassword;
using Vint.Core.Leagues.Components;
using Vint.Core.Logging;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Commands;
using Vint.Core.Squads;
using Vint.Core.User;
using Vint.Core.User.Components;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Autopilot.Connection;

public class BotConnection(
    int id,
    Database.Models.Player player,
    IServiceScope serviceScope
) : IPlayerConnection {
    ConcurrentHashSet<DelayedTask> DelayedTasks { get; } = [];
    public int Id { get; } = id;
    public ILogger Logger { get; private set; } = Log.Logger.ForType<BotConnection>().WithPlayer(player);
    public IServiceProvider ServiceProvider { get; } = serviceScope.ServiceProvider;
    public ConcurrentHashSet<IEntity> SharedEntities { get; } = [];

    public bool IsBot => true;
    public bool IsLoggedIn => IsConnected;
    public bool InSquad => false;
    public bool InLobby => LobbyPlayer != null;
    public bool Spectating => false;
    public bool IsConnected { get; private set; }

    bool WasInLobby { get; set; }

    public Database.Models.Player Player {
        get => field;
        set => throw new NotSupportedException();
    } = player;

    public UserContainer UserContainer { get; } = new(id, player);

    public LobbyPlayer? LobbyPlayer {
        get;
        set {
            field = value;
            WasInLobby = WasInLobby || value != null;
        }
    }

    public Squad? Squad {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public Spectator? Spectator {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public IEntity ClientSession {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public int BattleSeries {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public RestorePasswordData? RestorePasswordData {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public DateTimeOffset PingSendTime {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public DateTimeOffset PongReceiveTime {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public long Ping {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public Invite? Invite {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public Task Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task Login(bool saveAutoLoginToken, bool rememberMe, string hardwareFingerprint) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public async Task CalculateAndSetStatisticsByLobby(LobbyBase lobby) {
        Database.Models.Player[] players = lobby.Players.Select(p => p.Connection.Player).ToArray();

        double avgExp = players.Average(p => p.Experience);
        double avgRep = players.Average(p => p.Reputation);

        // magic numbers for standard deviation (idk how to get the real numbers)
        double devExp = 30000;
        double devRep = 500;

        if (players.Length > 1) {
            devExp = players.StandardDeviationBy(p => (double)p.Experience);
            devRep = players.StandardDeviationBy(p => (double)p.Reputation);
        }

        WyRandom random = new();

        int expDelta = (int)(ZigguratGaussian.Sample(random, avgExp, devExp) - Player.Experience);
        int repDelta = (int)(ZigguratGaussian.Sample(random, avgRep, devRep) - Player.Reputation);

        await ChangeExperience(expDelta);
        await ChangeReputation(repDelta);
    }

    public async Task ChangeReputation(int delta) {
        League oldLeague = Player.League;
        uint oldReputation = Player.Reputation;

        uint reputation = (uint)Math.Clamp(oldReputation + delta, 0, 99999);
        Player.Reputation = reputation;

        await UserContainer.Entity.ChangeComponent<UserReputationComponent>(component => component.Reputation = reputation);

        if (oldLeague != Player.League) {
            await UserContainer.Entity.RemoveComponent<LeagueGroupComponent>();
            await UserContainer.Entity.AddGroupComponent<LeagueGroupComponent>(Player.LeagueEntity);
        }
    }

    public async Task ChangeExperience(int delta) {
        Player.Experience = Math.Clamp(Player.Experience + delta, 0, Leveling.MaxExperience);
        await UserContainer.Entity.ChangeComponent<UserExperienceComponent>(component => component.Experience = Player.Experience);

        await CheckRankUp();
    }

    async Task CheckRankUp() {
        UserRankComponent rankComponent = UserContainer.Entity.GetComponent<UserRankComponent>();
        int oldRank = rankComponent.Rank;

        if (oldRank == Player.Rank) return;

        rankComponent.Rank = Player.Rank;
        await UserContainer.Entity.ChangeComponent(rankComponent);

        if (oldRank >= Player.Rank) return;

        if (InLobby && LobbyPlayer!.InRound)
            await LobbyPlayer.Round.Humans.Send<UpdateRankEvent>(UserContainer.Entity);
    }

    public Task ChangeGameplayChestScore(int delta) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task PurchaseItem(IEntity marketItem, int amount, int price, bool forXCrystals, bool mount) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task MountItem(IEntity userItem) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task AssembleModule(IEntity marketItem) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task UpgradeModule(IEntity userItem, bool forXCrystals) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task UpdateDeserterStatus(bool roundEnded, bool hasEnemies) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task CheckLoginRewards() {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task CheckPremiumBoostBonuses() {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task<bool> OwnsItem(IEntity marketItem) =>
        throw new NotSupportedException();

    public Task<bool> CanOwnItem(IEntity marketItem) =>
        throw new NotSupportedException();

    public async Task SetUsername(string username) {
        Logger.Warning("Changed username => '{New}'", username);
        Player.Username = username;
        Logger = Logger.WithPlayer(Player);
        await UserContainer.Entity.ChangeComponent<UserUidComponent>(component => component.Username = username);
    }

    public Task ChangeCrystals(long delta) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task ChangeXCrystals(long delta) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task SetGoldBoxes(int goldBoxes) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task DisplayMessage(string message, TimeSpan? closeTime = null) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task SetClipboard(string content) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task OpenURL(string url) {
        // Not supported by bots
        return Task.CompletedTask;
    }

    public Task Send(ICommand command) { // dummy implementation for BotConnection
        if (Logger.IsEnabled(LogEventLevel.Verbose))
            Logger.Verbose("Sending command: {Command}", command);
        return Task.CompletedTask;
    }

    public Task Send(IEvent @event) => Send(@event, []);

    public Task Send(IEvent @event, params IEnumerable<IEntity> entities) => Send(new SendEventCommand {
        Event = @event,
        Entities = entities as IEntity[] ?? entities.ToArray()
    });

    public Task Send<TEvent>() where TEvent : IEvent, new() => Send(new TEvent());

    public Task Send<TEvent>(params IEnumerable<IEntity> entities) where TEvent : IEvent, new() => Send(new TEvent(), entities);

    public Task Share(IEntity entity) => entity.Share(this);

    public async Task ShareIfUnshared(IEntity entity) {
        if (!SharedEntities.Contains(entity))
            await Share(entity);
    }

    public Task Unshare(IEntity entity) => entity.Unshare(this);

    public async Task UnshareIfShared(IEntity entity) {
        if (SharedEntities.Contains(entity))
            await Unshare(entity);
    }

    public void Schedule(TimeSpan delay, Func<Task> action) =>
        DelayedTasks.Add(new DelayedTask(DateTimeOffset.UtcNow + delay, action));

    public void Schedule(DateTimeOffset time, Func<Task> action) =>
        DelayedTasks.Add(new DelayedTask(time, action));

    public async Task Tick(CancellationToken cancellationToken = default) {
        foreach (DelayedTask delayedTask in DelayedTasks.Where(delayedTask => delayedTask.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            if (cancellationToken.IsCancellationRequested) return;

            await delayedTask.Task();
            DelayedTasks.TryRemove(delayedTask);
        }

        if (!InLobby && WasInLobby)
            await Disconnect();
    }

    public async Task Kick(string? reason) {
        Logger.Warning("Player kicked (reason: '{Reason}')", reason);
        await Disconnect();
    }

    async Task Disconnect() {
        if (!IsConnected) return;

        IsConnected = false;
        Logger.Information("Bot disconnected");

        try {
            await UserContainer.RemoveConnection(this);
            await UserContainer.Entity.RemoveComponent<UserOnlineComponent>();

            if (InLobby) {
                LobbyBase lobby = LobbyPlayer!.Lobby;

                if (LobbyPlayer.InRound) {
                    Round round = LobbyPlayer.Round;
                    await round.RemoveTanker(LobbyPlayer.Tanker);
                }

                await lobby.RemovePlayer(LobbyPlayer);
            }
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while disconnecting socket");
        } finally {
            GameServer gameServer = serviceScope.ServiceProvider.GetRequiredService<GameServer>();
            gameServer.RemovePlayer(Id);

            foreach (IEntity entity in SharedEntities)
                entity.SharedPlayers.TryRemove(this);

            SharedEntities.Clear();
        }

        await DisposeAsync();
    }

    public async Task OnConnected() {
        if (IsConnected) return;

        Logger.Information("Bot connected ({Id})", Id);
        IsConnected = true;
        await UserContainer.Entity.AddComponent<UserOnlineComponent>();
    }

    public override int GetHashCode() => Id;

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public override string ToString() =>
        $"BotConnection {{ Username: '{Player?.Username}' }}";

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    void Dispose(bool disposing) {
        if (disposing) {
            DelayedTasks.Clear();
            SharedEntities.Clear();
            serviceScope.Dispose();

            UserContainer.Entity.Dispose();
        }
    }

    async ValueTask DisposeAsyncCore() {
        DelayedTasks.Clear();
        SharedEntities.Clear();

        if (serviceScope is IAsyncDisposable ad)
            await ad.DisposeAsync();
        else serviceScope.Dispose();

        UserContainer.Entity.Dispose();
    }

    ~BotConnection() => Dispose(false);
}
