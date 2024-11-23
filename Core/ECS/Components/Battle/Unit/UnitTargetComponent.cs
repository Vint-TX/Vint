using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Unit;

[ProtocolId(1486455226183), ClientAddable, ClientRemovable]
public class UnitTargetComponent : IComponent {
    public IEntity Target { get; private set; } = null!;
    public IEntity TargetIncarnation { get; private set; } = null!;

    public Task Added(IPlayerConnection connection, IEntity entity) {
        switch (GetEffect(connection, entity)) {
            case DroneEffect drone:
                DroneAdded(drone);
                break;

            case SpiderMineEffect spider:
                SpiderAdded(spider);
                break;
        }

        return Task.CompletedTask;
    }

    public Task Removed(IPlayerConnection connection, IEntity entity) {
        switch (GetEffect(connection, entity)) {
            case SpiderMineEffect spider:
                SpiderRemoved(spider);
                break;
        }

        return Task.CompletedTask;
    }

    void DroneAdded(DroneEffect effect) =>
        ((DroneWeaponHandler)effect.WeaponHandler).IncarnationId = TargetIncarnation.Id;

    static void SpiderAdded(SpiderMineEffect effect) =>
        effect.State = SpiderState.Chasing;

    static void SpiderRemoved(SpiderMineEffect effect) =>
        effect.State = SpiderState.Idling;

    static WeaponEffect? GetEffect(IPlayerConnection connection, IEntity entity) =>
        connection
            .BattlePlayer
            ?.Tank
            ?.Effects
            .OfType<WeaponEffect>()
            .Single(effect => effect.Entity == entity);
}
