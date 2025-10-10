using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon.Shot.Railgun;

[ProtocolId(4963057750170414217)]
public class SelfRailgunChargingShotEvent : RailgunChargingShotEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        IEntity weaponEntity = entities.Single();
        IEntity tankEntity = EntityRegistry.Get(weaponEntity.GetComponent<TankGroupComponent>().Key);

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.ContainsKey(autopilotComponent.Id))
            return;

        Round round = human.Round;

        await round.Humans
            .Where(player => player != human)
            .Send(new RemoteRailgunChargingShotEvent { ClientTime = ClientTime }, weaponEntity);
    }
}
