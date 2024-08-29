using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Weapons;

public interface ITemperatureWeaponHandler {
    public IEntity MarketEntity { get; }
    public IEntity BattleEntity { get; }
    public float TemperatureLimit { get; }
    public float TemperatureDelta { get; }
    public TimeSpan TemperatureDuration { get; }
}

public class TemperatureAssist(
    BattleTank assistant,
    ITemperatureWeaponHandler weapon,
    float maxDamage,
    float currentTemperature,
    DateTimeOffset lastTick,
    TimeSpan duration
) {
    public BattleTank Assistant { get; } = assistant;
    public ITemperatureWeaponHandler Weapon { get; } = weapon;
    public float MaxDamage { get; } = maxDamage;
    public float CurrentTemperature { get; set; } = currentTemperature;
    public DateTimeOffset LastTick { get; set; } = lastTick;
    public TimeSpan Duration { get; set; } = duration;

    public override int GetHashCode() => Assistant.GetHashCode();
}
