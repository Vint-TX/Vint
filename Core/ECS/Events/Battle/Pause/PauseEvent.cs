using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Pause;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Pause;

[ProtocolId(-1316093147997460626)]
public class PauseEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.IsPaused)
            return;

        IEntity user = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer;

        battlePlayer.IsPaused = true;
        battlePlayer.KickTime = DateTimeOffset.UtcNow.AddMinutes(2);

        await user.AddComponent<PauseComponent>();
        await user.AddComponent(new IdleCounterComponent(0));
        await connection.Send(new IdleBeginTimeSyncEvent(DateTimeOffset.UtcNow), user);
    }
}
