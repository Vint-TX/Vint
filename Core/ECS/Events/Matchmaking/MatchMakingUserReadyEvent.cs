using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1496829083447)]
public class MatchMakingUserReadyEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.User.AddComponent(new MatchMakingUserReadyComponent());

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if (battlePlayer.Battle.StateManager.CurrentState is Running or WarmUp)
            battlePlayer.BattleJoinTime = DateTimeOffset.UtcNow.AddSeconds(2);
    }
}