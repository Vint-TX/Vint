using System.Numerics;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

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

    public async Task Added(IPlayerConnection connection, IEntity entity) {
        if (connection.LobbyPlayer?.Tanker?.Tank.WeaponHandler is not ShaftWeaponHandler shaft)
            return;

        await shaft.Aim();
    }

    public async Task Removed(IPlayerConnection connection, IEntity entity) {
        if (connection.LobbyPlayer?.Tanker?.Tank.WeaponHandler is not ShaftWeaponHandler shaft)
            return;

        await shaft.Idle();
    }
}
