using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.DailyBonus;
using Vint.Core.ECS.Components.Server.DailyBonus;
using Vint.Core.ECS.Components.Server.DailyBonus.Cycles;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;

namespace Vint.Core.Utils;

public static class DailyBonusUtils {
    static DailyBonusCommonConfigComponent CommonConfig { get; } = ConfigManager.GetComponent<DailyBonusCommonConfigComponent>("dailybonus");
    static DailyBonusFirstCycleComponent FirstCycleConfig { get; } = ConfigManager.GetComponent<DailyBonusFirstCycleComponent>("dailybonus");
    static DailyBonusEndlessCycleComponent EndlessCycleConfig { get; } = ConfigManager.GetComponent<DailyBonusEndlessCycleComponent>("dailybonus");

    public static DailyBonusCycleComponent GetCycleConfig(int cycle) =>
        cycle == 0 ? FirstCycleConfig : EndlessCycleConfig;

    public static bool RewardReceived(IPlayerConnection connection, int code) {
        List<int> receivedRewards = connection.UserContainer.Entity.GetComponent<UserDailyBonusReceivedRewardsComponent>().ReceivedRewards;
        return receivedRewards.Contains(code);
    }

    public static bool CurrentZoneCompleted(IPlayerConnection connection) {
        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        int cycle = player.DailyBonusCycle;
        int zone = player.DailyBonusZone;

        DailyBonusCycleComponent cycleConfig = GetCycleConfig(cycle);
        int maxCode = cycleConfig.Zones[zone] + 1;

        IEnumerable<int> availableRewardsInZone = cycleConfig.DailyBonuses.Select(bonus => bonus.Code).Where(code => code <= maxCode);
        List<int> receivedRewards = user.GetComponent<UserDailyBonusReceivedRewardsComponent>().ReceivedRewards;

        return receivedRewards.ContainsAll(availableRewardsInZone);
    }

    public static bool CurrentCycleCompleted(IPlayerConnection connection) {
        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        DailyBonusCycleComponent cycleConfig = GetCycleConfig(player.DailyBonusCycle);
        List<int> receivedRewards = user.GetComponent<UserDailyBonusReceivedRewardsComponent>().ReceivedRewards;

        return receivedRewards.ContainsAll(cycleConfig.DailyBonuses.Select(bonus => bonus.Code));
    }

    public static bool CanSwitchZone(IPlayerConnection connection) {
        Player player = connection.Player;
        DailyBonusCycleComponent cycleConfig = GetCycleConfig(player.DailyBonusCycle);

        return DailyBonusesUnlocked(connection) &&
               player.DailyBonusZone < cycleConfig.Zones.Length - 1 &&
               CurrentZoneCompleted(connection);
    }

    public static bool CanSwitchCycle(IPlayerConnection connection) {
        Player player = connection.Player;
        DailyBonusCycleComponent cycleConfig = GetCycleConfig(player.DailyBonusCycle);

        return DailyBonusesUnlocked(connection) &&
               player.DailyBonusZone == cycleConfig.Zones.Length - 1 &&
               CurrentCycleCompleted(connection);
    }

    public static bool TeleportCharged(IPlayerConnection connection) => // teleport is an alias for daily bonuses
        connection.Player.NextDailyBonusTime.Value <= DateTimeOffset.UtcNow &&
        DailyBonusesUnlocked(connection);

    public static bool DailyBonusesUnlocked(IPlayerConnection connection) =>
        connection.UserContainer.Entity.GetComponent<UserStatisticsComponent>().Statistics["BATTLES_PARTICIPATED"] >=
        CommonConfig.BattleCountToUnlockDailyBonuses;
}
