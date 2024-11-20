using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1498554483631)]
public class EnterBattleAsSpectatorFromLobbyRequestEvent : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        IBattleProcessor battleProcessor = serviceProvider.GetRequiredService<IBattleProcessor>();
        Battles.Battle? battle = battleProcessor.FindByBattleId(BattleId);

        if (connection.InLobby || battle == null) return;

        await battle.AddPlayer(connection, true);
    }
}
