using Vint.Core.Battles;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497356545125)]
public class StartBattleEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity lobby = entities.Single();
        Battles.Battle battle = connection.BattlePlayer!.Battle;
        BattleStateManager stateManager = battle.StateManager;
        
        switch (stateManager.CurrentState) {
            case NotStarted: {
                stateManager.SetState(new Starting(stateManager));
                break;
            }

            case Ended: {
                battle.Setup();
                stateManager.SetState(new Starting(stateManager));
                break;
            }

            default: {
                connection.BattlePlayer.Init();
                break;
            }
        }
    }
}