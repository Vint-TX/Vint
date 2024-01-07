using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(-5086569348607290080)]
public class ActivateTankEvent : IServerEvent {
    public long Phase { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.BattlePlayer is not { InBattleAsTank: true }) return;

        connection.BattlePlayer.Tank!.CollisionsPhase = Phase;
    }
}