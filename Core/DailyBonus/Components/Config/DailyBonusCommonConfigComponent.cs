namespace Vint.Core.ECS.Components.Server.DailyBonus;

public class DailyBonusCommonConfigComponent : IComponent {
    public int ReceivingBonusIntervalSec { get; private set; }
    public int BattleCountToUnlockDailyBonuses { get; private set; }
    public float PremiumTimeSpeedUp { get; private set; }
}
