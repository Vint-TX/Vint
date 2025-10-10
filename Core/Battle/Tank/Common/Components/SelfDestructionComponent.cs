using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Common.Components;

[ProtocolId(-9188485263407476652), ClientAddable]
public class SelfDestructionComponent : IComponent {
    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (!connection.InLobby || !connection.LobbyPlayer.InRound)
            return Task.CompletedTask;

        Tanker tanker = connection.LobbyPlayer.Tanker;

        tanker.Tank.IsSelfDestructing = true;
        return Task.CompletedTask;
    }
}
