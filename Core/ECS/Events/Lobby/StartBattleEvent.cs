using Vint.Core.Battles.States;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497356545125)]
public class StartBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        Battles.Battle battle = connection.BattlePlayer!.Battle;
        BattleStateManager stateManager = battle.StateManager;

        switch (stateManager.CurrentState) {
            case NotStarted or Ended: {
                await stateManager.SetState(new Starting(stateManager));
                break;
            }
        }
    }
}
