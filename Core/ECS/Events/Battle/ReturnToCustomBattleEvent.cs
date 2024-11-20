using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(1498743823980)]
public class ReturnToCustomBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || connection.BattlePlayer!.InBattle) return;

        await connection.BattlePlayer.Init();
    }
}
