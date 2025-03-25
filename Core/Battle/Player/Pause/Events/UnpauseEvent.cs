using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Pause;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Pause;

[ProtocolId(-3944419188146485646)]
public class UnpauseEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;

        if (tanker is not { IsPaused: true })
            return;

        IEntity battleUser = tanker.Tank.Entities.BattleUser;
        tanker.IsPaused = false;

        await battleUser.RemoveComponent<PauseComponent>();
        await battleUser.RemoveComponent<IdleCounterComponent>();
    }
}
