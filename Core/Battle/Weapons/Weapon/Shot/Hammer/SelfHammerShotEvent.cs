using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Weapons.Handlers.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon.Shot.Hammer;

[ProtocolId(-1937089974629265090)]
public class SelfHammerShotEvent : SelfShotEvent {
    public int RandomSeed { get; private set; }

    protected override RemoteHammerShotEvent RemoteEvent => new() {
        RandomSeed = RandomSeed,
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public override async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        BotTanker? bot = null;
        IEntity weaponEntity = entities.Single();
        IEntity tankEntity = EntityRegistry.Get(weaponEntity.GetComponent<TankGroupComponent>().Key);

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.TryGetValue(autopilotComponent.Id, out bot))
            return;

        BattleTank tank = bot?.Tank ?? human.Tank;

        if (tank.WeaponHandler is not HammerWeaponHandler hammer || hammer.BattleEntity != weaponEntity)
            return;

        await base.Execute(connection, entities);
        await hammer.SetCurrentCartridgeCount(hammer.CurrentCartridgeCount - 1);
    }
}
