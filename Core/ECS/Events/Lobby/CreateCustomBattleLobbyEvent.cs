using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496750075382)]
public class CreateCustomBattleLobbyEvent : IServerEvent {
    public BattleProperties Properties { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (connection.InLobby) return;

        IBattleProcessor battleProcessor = serviceProvider.GetRequiredService<IBattleProcessor>();
        Battles.Battle battle = battleProcessor.CreateCustomBattle(Properties, connection);
        await battle.AddPlayer(connection);
    }
}
