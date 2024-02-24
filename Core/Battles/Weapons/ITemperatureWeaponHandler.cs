using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public interface ITemperatureWeaponHandler {
    public float TemperatureLimit { get; }
    public float TemperatureDelta { get; }
}

public class TemperatureAssist( // todo modules
    BattleTank assistant,
    ITemperatureWeaponHandler weapon,
    float maxDamage,
    float currentTemperature,
    DateTimeOffset lastTick
) {
    public BattleTank Assistant { get; init; } = assistant;
    public ITemperatureWeaponHandler Weapon { get; init; } = weapon;
    public float MaxDamage { get; init; } = maxDamage;
    public float CurrentTemperature { get; set; } = currentTemperature;
    public DateTimeOffset LastTick { get; set; } = lastTick;

    public override int GetHashCode() => Assistant.GetHashCode();
}