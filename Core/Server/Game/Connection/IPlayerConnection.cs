using System.Diagnostics.CodeAnalysis;
using ConcurrentCollections;
using Serilog;
using Vint.Core.Battle.Player;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.RestorePassword;
using Vint.Core.Server.Game.Protocol.Commands;
using Vint.Core.Squads;
using Vint.Core.User;

namespace Vint.Core.Server.Game.Connection;

public interface IPlayerConnection : IAsyncDisposable, IDisposable {
    int Id { get; }

    ILogger Logger { get; }

    Player Player { get; set; }
    Squad? Squad { get; set; }
    LobbyPlayer? LobbyPlayer { get; set; }
    Spectator? Spectator { get; set; }
    UserContainer UserContainer { get; }
    IEntity ClientSession { get; }
    IServiceProvider ServiceProvider { get; }

    bool IsBot { get; }
    bool IsLoggedIn { get; }
    [MemberNotNullWhen(true, nameof(Squad))]
    bool InSquad { get; }
    [MemberNotNullWhen(true, nameof(LobbyPlayer))]
    bool InLobby { get; }
    [MemberNotNullWhen(true, nameof(Spectator))]
    bool Spectating { get; }
    DateTimeOffset PingSendTime { set; }
    DateTimeOffset PongReceiveTime { set; }
    long Ping { get; }
    Invite? Invite { get; set; }
    RestorePasswordData? RestorePasswordData { get; set; }

    int BattleSeries { get; set; }

    ConcurrentHashSet<IEntity> SharedEntities { get; }

    Task Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration);

    Task Login(bool saveAutoLoginToken, bool rememberMe, string hardwareFingerprint);

    Task ChangeReputation(int delta);

    Task ChangeExperience(int delta);

    Task ChangeGameplayChestScore(int delta);

    Task PurchaseItem(IEntity marketItem, int amount, int price, bool forXCrystals, bool mount);

    Task MountItem(IEntity userItem);

    Task AssembleModule(IEntity marketItem);

    Task UpgradeModule(IEntity userItem, bool forXCrystals);

    Task CheckLoginRewards();

    Task CheckPremiumBoostBonuses();

    Task UpdateDeserterStatus(bool roundEnded, bool hasEnemies);

    Task<bool> OwnsItem(IEntity marketItem);

    Task<bool> CanOwnItem(IEntity marketItem);

    Task SetUsername(string username);

    Task ChangeCrystals(long delta);

    Task ChangeXCrystals(long delta);

    Task SetGoldBoxes(int goldBoxes);

    Task DisplayMessage(string message, TimeSpan? closeTime = null);

    Task SetClipboard(string content);

    Task OpenURL(string url);

    Task Kick(string? reason);

    Task Send(ICommand command);

    Task Send(IEvent @event);

    Task Send(IEvent @event, params IEnumerable<IEntity> entities);

    Task Send<TEvent>() where TEvent : IEvent, new();

    Task Send<TEvent>(params IEnumerable<IEntity> entities) where TEvent : IEvent, new();

    Task Share(IEntity entity);

    Task ShareIfUnshared(IEntity entity);

    Task Unshare(IEntity entity);

    Task UnshareIfShared(IEntity entity);

    void Schedule(TimeSpan delay, Func<Task> action);

    void Schedule(DateTimeOffset time, Func<Task> action);

    Task Tick(CancellationToken cancellationToken = default);

    Task OnConnected();
}
