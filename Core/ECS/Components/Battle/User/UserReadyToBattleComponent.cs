using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.User;

[ProtocolId(1399558738794728790), ClientAddable]
public class UserReadyToBattleComponent : IComponent {
    public Task Added(IPlayerConnection connection, IEntity entity) {
        TankStateManager? stateManager = connection.BattlePlayer?.Tank?.StateManager;
        stateManager?.SetState(new Spawn(stateManager));
        return Task.CompletedTask;
    }
}
