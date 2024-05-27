using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(635890723433891050)]
public class RequestLoadBattleInfoEvent : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        Battles.Battle? battle = connection.Server.BattleProcessor.FindByBattleId(BattleId);

        if (battle == null) return;

        await connection.Send(new BattleInfoForLabelLoadedEvent(battle.MapEntity, battle.Id, battle.Properties.BattleMode), entities.Single());
    }
}
