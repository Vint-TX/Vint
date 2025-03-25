using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.User;

[ProtocolId(1399558738794728790), ClientAddable]
public class UserReadyToBattleComponent : IComponent {
    public async Task Added(IPlayerConnection connection, IEntity entity) {
        TankStateManager? stateManager = connection.LobbyPlayer?.Tanker?.Tank.StateManager;

        if (stateManager == null)
            return;

        await stateManager.SetState(new Spawn(stateManager));
    }
}
