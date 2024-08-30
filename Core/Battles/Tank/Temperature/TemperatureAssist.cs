using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Tank.Temperature;

public class TemperatureAssist(
    BattleTank tank,
    IEntity weaponMarketEntity,
    IEntity weaponBattleEntity,
    TimeSpan duration,
    float delta,
    float limit
) {
    public BattleTank Tank { get; } = tank;
    public IEntity WeaponMarketEntity { get; } = weaponMarketEntity;
    public IEntity WeaponBattleEntity { get; } = weaponBattleEntity;
    public float Limit { get; } = limit;
    public int LimitSign { get; } = Math.Sign(limit);

    public TimeSpan CurrentDuration { get; set; } = duration;
    public float CurrentDelta { get; set; } = delta;

    public bool CanMerge(TemperatureAssist other) =>
        LimitSign == other.LimitSign &&
        Tank == other.Tank &&
        WeaponBattleEntity == other.WeaponMarketEntity;

    public void MergeWith(TemperatureAssist other) {
        if (!CanMerge(other))
            throw new InvalidOperationException("Cannot merge assists");

        CurrentDelta += other.CurrentDelta;
        Clamp();
    }

    public void Clamp() =>
        CurrentDelta = Limit < 0
            ? Math.Clamp(CurrentDelta, Limit, 0)
            : Math.Clamp(CurrentDelta, 0, Limit);

    public void Subtract(float delta) {
        CurrentDelta -= LimitSign * Math.Abs(delta);
        Clamp();
    }
}

public class HeatTemperatureAssist(
    BattleTank tank,
    TimeSpan duration,
    float delta,
    float limit,
    IEntity weaponMarketEntity,
    IEntity weaponBattleEntity,
    float heatDamage
) : TemperatureAssist(tank, weaponMarketEntity, weaponBattleEntity, duration, delta, limit) {
    public float HeatDamage { get; } = heatDamage;
}
