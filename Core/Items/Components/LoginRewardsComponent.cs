using Vint.Core.ECS.Components.Notification;

namespace Vint.Core.ECS.Components.Server.Login;

public class LoginRewardsComponent : IComponent {
    public float IntervalInSeconds { get; private set; }
    public int BattleCountToUnlock { get; private set; }
    public List<LoginRewardItem> Rewards { get; private set; } = null!;

    public int MaxDay => Rewards.Max(reward => reward.Day);

    public IEnumerable<LoginRewardItem> GetRewardsByDay(int day) =>
        Rewards.Where(reward => reward.Day == day);
}
