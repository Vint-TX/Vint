using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Pause;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Pause;

[ProtocolId(-3944419188146485646)]
public class UnpauseEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            !connection.BattlePlayer.IsPaused)
            return;

        IEntity user = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer;

        battlePlayer.IsPaused = false;
        battlePlayer.KickTime = null;

        await user.RemoveComponent<PauseComponent>();
        await user.RemoveComponent<IdleCounterComponent>();
    }
}
