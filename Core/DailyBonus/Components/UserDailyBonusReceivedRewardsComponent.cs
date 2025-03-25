using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Components;

[ProtocolId(636459174909060087)]
public class UserDailyBonusReceivedRewardsComponent(
    List<int> receivedRewards
) : PrivateComponent {
    public List<int> ReceivedRewards { get; } = receivedRewards;
}
