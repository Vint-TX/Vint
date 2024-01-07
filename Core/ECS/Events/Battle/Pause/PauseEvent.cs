using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Pause;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Pause;

[ProtocolId(-1316093147997460626)]
public class PauseEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InBattle || connection.BattlePlayer!.Paused) return;

        IEntity user = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer;

        battlePlayer.Paused = true;
        battlePlayer.KickTime = DateTimeOffset.UtcNow.AddMinutes(2);

        user.AddComponent(new PauseComponent());
        user.AddComponent(new IdleCounterComponent(0));
        connection.Send(new IdleBeginTimeSyncEvent(DateTimeOffset.UtcNow), user);
    }
}