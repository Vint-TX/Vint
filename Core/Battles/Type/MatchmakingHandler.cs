using ConcurrentCollections;
using LinqToDB;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Type;

public class MatchmakingHandler : TypeHandler {
    public MatchmakingHandler(Battle battle) : base(battle) =>
        Maps = ConfigManager.MapInfos.Where(map => map.MatchMaking && map.HasSpawnPoints(BattleMode)).ToList();

    public BattleMode BattleMode { get; } = GetRandomMode();

    ConcurrentHashSet<BattlePlayer> WaitingPlayers { get; } = [];
    List<MapInfo> Maps { get; }

    public override Task Setup() {
        MapInfo mapInfo = Maps.Shuffle().First();

        Battle.Properties = new BattleProperties(
            BattleMode,
            GravityType.Earth,
            mapInfo.Id,
            false,
            true,
            true,
            false,
            mapInfo.MaxPlayers,
            10,
            100);

        Battle.MapInfo = mapInfo;
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == mapInfo.Id);
        Battle.LobbyEntity = new MatchMakingLobbyTemplate().Create(
            Battle.Properties,
            Battle.MapEntity);

        return Task.CompletedTask;
    }

    public override async Task Tick() {
        foreach (BattlePlayer battlePlayer in WaitingPlayers.Where(player => DateTimeOffset.UtcNow >= player.BattleJoinTime)) {
            await battlePlayer.Init();
            WaitingPlayers.TryRemove(battlePlayer);
        }
    }

    public override async Task PlayerEntered(BattlePlayer battlePlayer) {
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        IEntity user = connection.User;

        await user.AddComponent<MatchMakingUserComponent>();

        if (Battle.StateManager.CurrentState is not (WarmUp or Running)) return;

        await connection.Send(new MatchMakingLobbyStartTimeEvent(battlePlayer.BattleJoinTime), user);
        WaitingPlayers.Add(battlePlayer);
    }

    public override async Task PlayerExited(BattlePlayer battlePlayer) {
        bool battleEnded = Battle.StateManager.CurrentState is Ended;

        WaitingPlayers.TryRemove(battlePlayer);
        await battlePlayer.PlayerConnection.User.RemoveComponentIfPresent<MatchMakingUserComponent>();

        await UpdateDeserterStatus(battlePlayer, battleEnded);
        await CheckLoginReward(battlePlayer, battleEnded);
    }

    async Task UpdateDeserterStatus(BattlePlayer battlePlayer, bool battleEnded) {
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        Database.Models.Player player = connection.Player;
        IEntity user = connection.User;

        BattleLeaveCounterComponent battleLeaveCounter = user.GetComponent<BattleLeaveCounterComponent>();
        long lefts = battleLeaveCounter.Value;
        int needGoodBattles = battleLeaveCounter.NeedGoodBattles;

        if (battleEnded) {
            if (needGoodBattles > 0)
                needGoodBattles--;

            if (needGoodBattles == 0)
                lefts = 0;
        } else {
            bool hasEnemies = Battle.Players.Any(other => other.InBattleAsTank && other.Tank!.IsEnemy(battlePlayer.Tank!));

            if (hasEnemies) {
                connection.BattleSeries = 0;
                lefts++;

                if (lefts >= 2)
                    needGoodBattles = needGoodBattles > 0 ? (int)lefts / 2 : 2;
            }
        }

        battleLeaveCounter.Value = lefts;
        battleLeaveCounter.NeedGoodBattles = needGoodBattles;

        player.DesertedBattlesCount = battleLeaveCounter.Value;
        player.NeedGoodBattlesCount = battleLeaveCounter.NeedGoodBattles;

        await using DbConnection db = new();
        await db.Players
            .Where(p => p.Id == player.Id)
            .Set(p => p.DesertedBattlesCount, player.DesertedBattlesCount)
            .Set(p => p.NeedGoodBattlesCount, player.NeedGoodBattlesCount)
            .UpdateAsync();

        await user.ChangeComponent(battleLeaveCounter);
    }

    async Task CheckLoginReward(BattlePlayer battlePlayer, bool battleEnded) {
        if (!battleEnded) return;

        IPlayerConnection connection = battlePlayer.PlayerConnection;
        Database.Models.Player player = connection.Player;
        LoginRewardsComponent loginRewardsComponent = ConfigManager.GetComponent<LoginRewardsComponent>("login_rewards");

        await using DbConnection db = new();
        long battles = await db.Statistics
            .Where(stats => stats.PlayerId == player.Id)
            .Select(stats => stats.BattlesParticipated)
            .SingleOrDefaultAsync();

        if (player.NextLoginRewardTime > DateTimeOffset.UtcNow ||
            player.LastLoginRewardDay >= loginRewardsComponent.MaxDay ||
            battles < loginRewardsComponent.BattleCountToUnlock) return;

        int day = player.LastLoginRewardDay + 1;
        List<LoginRewardItem> loginRewards = Leveling.GetLoginRewards(day).ToList();

        foreach (LoginRewardItem reward in loginRewards) {
            IEntity? entity = connection.GetEntity(reward.MarketItemEntity);

            if (entity == null) continue;

            await connection.PurchaseItem(entity, reward.Amount, 0, false, false);
        }

        player.LastLoginRewardDay = day;
        player.LastLoginRewardTime = DateTimeOffset.UtcNow;

        await db.Players
            .Where(p => p.Id == player.Id)
            .Set(p => p.LastLoginRewardDay, player.LastLoginRewardDay)
            .Set(p => p.LastLoginRewardTime, player.LastLoginRewardTime)
            .UpdateAsync();

        IEntity notification = new LoginRewardNotificationTemplate().Create(loginRewards, loginRewardsComponent.Rewards, day);
        await connection.Share(notification);
    }
}
