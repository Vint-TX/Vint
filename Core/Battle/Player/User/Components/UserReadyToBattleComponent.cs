using Vint.Core.Battle.Tank.State;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.User.Components;

[ProtocolId(1399558738794728790), ClientAddable]
public class UserReadyToBattleComponent : IComponent {
    public async Task Added(IPlayerConnection connection, IEntity entity) {
        TankStateManager? stateManager = connection.LobbyPlayer?.Tanker?.Tank.StateManager;

        if (stateManager == null)
            return;

        await stateManager.SetState(new Spawn(stateManager));
    }
}
