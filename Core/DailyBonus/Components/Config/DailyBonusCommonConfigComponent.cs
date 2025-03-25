using Vint.Core.ECS.Components;

namespace Vint.Core.DailyBonus.Components.Config;

public class DailyBonusCommonConfigComponent : IComponent {
    public int ReceivingBonusIntervalSec { get; private set; }
    public int BattleCountToUnlockDailyBonuses { get; private set; }
    public float PremiumTimeSpeedUp { get; private set; }
}
