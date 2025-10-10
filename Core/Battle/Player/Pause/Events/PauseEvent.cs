using Vint.Core.Battle.Player.Pause.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Pause.Events;

[ProtocolId(-1316093147997460626)]
public class PauseEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;

        if (tanker?.Tank is not { IsPaused: false })
            return;

        IEntity battleUser = tanker.Tank.Entities.BattleUser;
        tanker.Tank.IsPaused = true;

        await battleUser.AddComponent<PauseComponent>();
        await battleUser.AddComponent(new IdleCounterComponent(0));
        await connection.Send(new IdleBeginTimeSyncEvent(DateTimeOffset.UtcNow), battleUser);
    }
}
