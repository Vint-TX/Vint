using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Weapons.Weapon.Hit;

[ProtocolId(1430210549752)]
public class SelfUpdateStreamHitEvent : UpdateStreamHitEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity weaponEntity = entities.Single();
        IEntity tankEntity;

        if (connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        if (weaponEntity.TryGetComponent(out TankGroupComponent? tankGroupComponent))
            tankEntity = EntityRegistry.Get(tankGroupComponent.Key);
        else { // ECS...... :(
            long userId = weaponEntity.GetComponent<UserGroupComponent>().Key;
            Tanker? tanker = human.Round.Tankers.FirstOrDefault(tanker => tanker.Connection.UserContainer.Id == userId);

            if (tanker == null) return;

            tankEntity = tanker.Tank.Entities.Tank;
        }

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.ContainsKey(autopilotComponent.Id))
            return;

        await human.Round.Humans
            .Where(player => player != human)
            .Send(new RemoteUpdateStreamHitEvent {
                StaticHit = StaticHit,
                TankHit = TankHit
            }, weaponEntity);
    }
}
