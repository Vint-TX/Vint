using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(1523423182023)]
public class LoginRewardsNotificationComponent(
    List<LoginRewardItem> currentRewards,
    List<LoginRewardItem> allRewards,
    int currentDay
) : IComponent {
    public List<LoginRewardItem> CurrentRewards { get; private set; } = currentRewards;
    public List<LoginRewardItem> AllRewards { get; private set; } = allRewards;
    public int CurrentDay { get; private set; } = currentDay;
}

public class LoginRewardItem {
    public long MarketItemEntity { get; set; }
    public int Amount { get; set; }
    public int Day { get; set; }
}
