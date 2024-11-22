using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Weapons;

public interface ITemperatureWeaponHandler {
    IEntity MarketEntity { get; }
    IEntity BattleEntity { get; }
    float TemperatureLimit { get; }
    float TemperatureDelta { get; }
    TimeSpan TemperatureDuration { get; }
}
