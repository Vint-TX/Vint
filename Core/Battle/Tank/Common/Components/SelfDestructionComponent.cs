using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle;

[ProtocolId(-9188485263407476652), ClientAddable]
public class SelfDestructionComponent : IComponent {
    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (!connection.InLobby || !connection.LobbyPlayer.InRound)
            return Task.CompletedTask;

        Tanker tanker = connection.LobbyPlayer.Tanker;

        tanker.Tank.SelfDestructTime = DateTimeOffset.UtcNow.AddSeconds(tanker.SelfDestructionConfig.SuicideDurationTime);
        return Task.CompletedTask;
    }
}
