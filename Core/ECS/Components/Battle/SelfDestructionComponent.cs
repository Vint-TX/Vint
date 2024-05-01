using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle;

[ProtocolId(-9188485263407476652), ClientAddable]
public class SelfDestructionComponent : IComponent {
    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return Task.CompletedTask;

        connection.BattlePlayer.Tank!.SelfDestructTime = DateTimeOffset.UtcNow.AddSeconds(5);
        return Task.CompletedTask;
    }
}
