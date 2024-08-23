using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Unit;

[ProtocolId(1486455226183), ClientAddable, ClientRemovable]
public class UnitTargetComponent : IComponent {
    public IEntity Target { get; private set; } = null!;
    public IEntity TargetIncarnation { get; private set; } = null!;

    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer
                ?.Tank
                ?.Effects
                .OfType<WeaponEffect>()
                .Single(effect => effect.Entity == entity)
                .WeaponHandler is not DroneWeaponHandler weaponHandler)
            return Task.CompletedTask;

        weaponHandler.IncarnationId = TargetIncarnation.Id;
        return Task.CompletedTask;
    }
}
