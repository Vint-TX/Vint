using Vint.Core.ECS.Components.Notification;

namespace Vint.Core.ECS.Components.Server;

public class LoginRewardsComponent : IComponent {
    public float IntervalInSeconds { get; private set; }
    public int BattleCountToUnlock { get; private set; }
    public List<LoginRewardItem> Rewards { get; private set; } = null!;

    public int MaxDay => Rewards.Max(reward => reward.Day);
}
