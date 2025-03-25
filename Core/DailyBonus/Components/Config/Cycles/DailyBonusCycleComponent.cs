using JetBrains.Annotations;
using Vint.Core.ECS.Enums;

namespace Vint.Core.ECS.Components.Server.DailyBonus.Cycles;

public abstract class DailyBonusCycleComponent : IComponent {
    public int[] Zones { get; protected set; } = null!;
    public DailyBonusData[] DailyBonuses { get; protected set; } = null!;
}

[UsedImplicitly]
public class DailyBonusData {
    public DailyBonusType DailyBonusType {
        get {
            if (field != DailyBonusType.None)
                return field;

            if (ContainerReward != null) field = DailyBonusType.Container;
            if (DetailReward != null) field = DailyBonusType.Detail;
            if (CryAmount > 0) field = DailyBonusType.Cry;
            if (XCryAmount > 0) field = DailyBonusType.XCry;
            if (EnergyAmount > 0) field = DailyBonusType.Energy;

            return field;
        }
    }

    public int Code { get; set; }

    public long? CryAmount { get; set; }
    public long? XCryAmount { get; set; }
    public long? EnergyAmount { get; set; } // energy is not used in the game anymore
    public DailyBonusGarageItemReward? ContainerReward { get; set; }
    public DailyBonusGarageItemReward? DetailReward { get; set; }

    public bool IsEpic() => DailyBonusType is DailyBonusType.Container
        or DailyBonusType.Detail
        or DailyBonusType.XCry
        or DailyBonusType.Energy;
}

[UsedImplicitly]
public class DailyBonusGarageItemReward {
    public long MarketItemId { get; set; }
    public int Amount { get; set; }
}
