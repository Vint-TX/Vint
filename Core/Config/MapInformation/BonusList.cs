using Vint.Core.Battles.Bonus;

namespace Vint.Core.Config.MapInformation;

public readonly record struct BonusList(
    List<Bonus> Repair,
    List<Bonus> Armor,
    List<Bonus> Damage,
    List<Bonus> Speed,
    List<Bonus> Gold
) {
    public IDictionary<BonusType, IEnumerable<Bonus>> ToDictionary() => new Dictionary<BonusType, IEnumerable<Bonus>> {
        { BonusType.Repair, Repair },
        { BonusType.Armor, Armor },
        { BonusType.Damage, Damage },
        { BonusType.Speed, Speed },
        { BonusType.Gold, Gold }
    };
}