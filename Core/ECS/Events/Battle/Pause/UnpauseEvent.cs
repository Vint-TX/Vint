using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Pause;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Pause;

[ProtocolId(-3944419188146485646)]
public class UnpauseEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InBattle || !connection.BattlePlayer!.Paused) return;

        IEntity user = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer;

        battlePlayer.Paused = false;
        battlePlayer.KickTime = null;

        user.RemoveComponent<PauseComponent>();
        user.RemoveComponent<IdleCounterComponent>();
    }
}