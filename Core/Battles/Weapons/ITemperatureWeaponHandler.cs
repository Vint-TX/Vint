using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Weapons;

public interface ITemperatureWeaponHandler {
    public IEntity MarketEntity { get; }
    public IEntity BattleEntity { get; }
    public float TemperatureLimit { get; }
    public float TemperatureDelta { get; }
    public TimeSpan TemperatureDuration { get; }
}
