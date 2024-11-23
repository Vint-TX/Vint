using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(635890723433891050)]
public class RequestLoadBattleInfoEvent : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        IBattleProcessor battleProcessor = serviceProvider.GetRequiredService<IBattleProcessor>();

        Battles.Battle? battle = battleProcessor.FindByBattleId(BattleId);

        if (battle == null) return;

        await connection.Send(new BattleInfoForLabelLoadedEvent(battle.MapEntity, battle.Id, battle.Properties.BattleMode), entities.Single());
    }
}
