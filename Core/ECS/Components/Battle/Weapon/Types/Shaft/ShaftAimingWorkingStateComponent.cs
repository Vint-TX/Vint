using System.Numerics;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(4186891190183470299), ClientAddable, ClientRemovable]
public class ShaftAimingWorkingStateComponent : IComponent {
    public float InitialEnergy { get; private set; }
    public float ExhaustedEnergy { get; private set; }
    public float VerticalAngle { get; private set; }
    public Vector3 WorkingDirection { get; private set; }
    public float VerticalSpeed { get; private set; }
    public int VerticalElevationDir { get; private set; }
    public bool IsActive { get; private set; }
    public int ClientTime { get; private set; }

    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not ShaftWeaponHandler shaft) return Task.CompletedTask;

        shaft.Aim();
        return Task.CompletedTask;
    }

    public Task Removed(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not ShaftWeaponHandler shaft) return Task.CompletedTask;

        shaft.Idle();
        return Task.CompletedTask;
    }
}
