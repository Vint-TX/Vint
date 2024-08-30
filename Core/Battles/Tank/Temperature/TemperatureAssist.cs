using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Tank.Temperature;

public class TemperatureAssist(
    BattleTank tank,
    TimeSpan duration,
    float delta,
    float limit
) {
    public BattleTank Tank { get; } = tank;
    public float Limit { get; } = limit;
    public int Sign { get; } = Math.Sign(limit);

    public TimeSpan CurrentDuration { get; set; } = duration;
    public float CurrentDelta { get; set; } = delta;
}

public class HeatTemperatureAssist(
    BattleTank tank,
    TimeSpan duration,
    float delta,
    float limit,
    IEntity weaponMarketEntity,
    IEntity weaponBattleEntity,
    float heatDamage
) : TemperatureAssist(tank, duration, delta, limit) {
    public IEntity WeaponMarketEntity { get; } = weaponMarketEntity;
    public IEntity WeaponBattleEntity { get; } = weaponBattleEntity;
    public float HeatDamage { get; } = heatDamage;
}
