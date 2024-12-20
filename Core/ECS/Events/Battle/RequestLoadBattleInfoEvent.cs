using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(635890723433891050)]
public class RequestLoadBattleInfoEvent(
    IBattleProcessor battleProcessor
) : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Battles.Battle? battle = battleProcessor.FindByBattleId(BattleId);

        if (battle == null) return;

        await connection.Send(new BattleInfoForLabelLoadedEvent(battle.MapEntity, battle.Id, battle.Properties.BattleMode), entities.Single());
    }
}
