using System.Numerics;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(4186891190183470299)]
public class ShaftAimingWorkingStateComponent : IComponent { // todo
    public float InitialEnergy { get; private set; }
    public float ExhaustedEnergy { get; private set; }
    public float VerticalAngle { get; private set; }
    public Vector3 WorkingDirection { get; private set; }
    public float VerticalSpeed { get; private set; }
    public int VerticalElevationDir { get; private set; }
    public bool IsActive { get; private set; }
    public int ClientTime { get; private set; }

    public void Added(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not ShaftWeaponHandler shaft) return;
        shaft.Aim();
    }

    public void Removed(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not ShaftWeaponHandler shaft) return;
        shaft.Idle();
    }
}