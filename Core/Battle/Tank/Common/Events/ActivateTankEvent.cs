using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(-5086569348607290080)]
public class ActivateTankEvent : IServerEvent {
    public long Phase { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) {
        BattleTank? tank = connection.LobbyPlayer?.Tanker?.Tank;

        if (tank == null)
            return Task.CompletedTask;

        tank.CollisionsPhase = Phase;
        return Task.CompletedTask;
    }
}
