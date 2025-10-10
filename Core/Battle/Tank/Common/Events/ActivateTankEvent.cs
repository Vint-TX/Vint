using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Common.Events;

[ProtocolId(-5086569348607290080)]
public class ActivateTankEvent : IServerEvent {
    public long Phase { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity tank = entities.Single();

        if (!tank.HasComponent<TankComponent>() ||
            connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return Task.CompletedTask;

        if (!tank.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            human.Tank.CollisionsPhase = Phase;
        } else if (human.ControlledBots.TryGetValue(autopilotComponent.Id, out BotTanker? bot)) {
            bot.Tank.CollisionsPhase = Phase;
        }

        return Task.CompletedTask;
    }
}
