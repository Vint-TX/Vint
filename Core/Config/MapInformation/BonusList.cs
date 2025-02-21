using Vint.Core.Battle.Bonus;

namespace Vint.Core.Config.MapInformation;

public readonly record struct BonusList(
    List<BonusInfo> Repair,
    List<BonusInfo> Armor,
    List<BonusInfo> Damage,
    List<BonusInfo> Speed,
    List<BonusInfo> Gold
) {
    public Dictionary<BonusType, IEnumerable<BonusInfo>> ToDictionary() => new() {
        { BonusType.Repair, Repair },
        { BonusType.Armor, Armor },
        { BonusType.Damage, Damage },
        { BonusType.Speed, Speed },
        { BonusType.Gold, Gold }
    };
}
