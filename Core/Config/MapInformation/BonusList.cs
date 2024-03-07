using Vint.Core.Battles.Bonus;

namespace Vint.Core.Config.MapInformation;

public class BonusList {
    public IList<Bonus> Repair { get; set; } = null!;
    public IList<Bonus> Armor { get; set; } = null!;
    public IList<Bonus> Damage { get; set; } = null!;
    public IList<Bonus> Speed { get; set; } = null!;
    public IList<Bonus> Gold { get; set; } = null!;

    public IDictionary<BonusType, IEnumerable<Bonus>> ToDictionary() => new Dictionary<BonusType, IEnumerable<Bonus>> {
        { BonusType.Repair, Repair },
        { BonusType.Armor, Armor },
        { BonusType.Damage, Damage },
        { BonusType.Speed, Speed },
        { BonusType.Gold, Gold }
    };
}