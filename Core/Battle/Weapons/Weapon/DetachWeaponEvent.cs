using System.Numerics;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Tank.State;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon;

[ProtocolId(1447917521601)]
public class DetachWeaponEvent : IServerEvent {
    public Vector3 AngularVelocity { get; private set; }
    public Vector3 Velocity { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity tankEntity = entities.Single();

        if (!tankEntity.HasComponent<TankComponent>() ||
            connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        BotTanker? bot = null;

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.TryGetValue(autopilotComponent.Id, out bot))
            return;

        Round round = human.Round;
        BattleTank tank = bot?.Tank ?? human.Tank;

        if (tank.StateManager.CurrentState is not Dead)
            return;

        await round.Humans.Send(this, tankEntity);
    }
}
