using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1496829083447)]
public class MatchMakingUserReadyEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.User.AddComponentIfAbsent<MatchMakingUserReadyComponent>();

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if (battlePlayer.Battle.StateManager.CurrentState is not (Running or WarmUp)) return Task.CompletedTask;

        battlePlayer.BattleJoinTime = DateTimeOffset.UtcNow.AddSeconds(3);
        connection.Send(new MatchMakingLobbyStartTimeEvent(battlePlayer.BattleJoinTime), connection.User);
        return Task.CompletedTask;
    }
}
