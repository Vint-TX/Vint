using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1496829083447)]
public class MatchMakingUserReadyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        await connection.User.AddComponentIfAbsent<MatchMakingUserReadyComponent>();

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if (battlePlayer.Battle.StateManager.CurrentState is not (Running or WarmUp)) return;

        battlePlayer.BattleJoinTime = DateTimeOffset.UtcNow.AddSeconds(3);
        await connection.Send(new MatchMakingLobbyStartTimeEvent(battlePlayer.BattleJoinTime), connection.User);
    }
}
