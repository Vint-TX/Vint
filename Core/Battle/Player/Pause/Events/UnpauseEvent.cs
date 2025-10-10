using Vint.Core.Battle.Player.Pause.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Pause.Events;

[ProtocolId(-3944419188146485646)]
public class UnpauseEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;

        if (tanker?.Tank is not { IsPaused: true })
            return;

        IEntity battleUser = tanker.Tank.Entities.BattleUser;
        tanker.Tank.IsPaused = false;

        await battleUser.RemoveComponent<PauseComponent>();
        await battleUser.RemoveComponent<IdleCounterComponent>();
    }
}
