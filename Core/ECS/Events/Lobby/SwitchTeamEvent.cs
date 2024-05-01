using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1499172594697)]
public class SwitchTeamEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return Task.CompletedTask;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (battle.ModeHandler is not TeamHandler teamHandler) return Task.CompletedTask;

        UserLimitComponent userLimitComponent = battle.LobbyEntity.GetComponent<UserLimitComponent>();
        TeamColor prevColor = battlePlayer.TeamColor;
        int newTeamPlayersCount = prevColor == TeamColor.Red ? teamHandler.BluePlayers.Count() : teamHandler.RedPlayers.Count();

        if (newTeamPlayersCount >= userLimitComponent.TeamLimit) return Task.CompletedTask;

        connection.User.RemoveComponent<TeamColorComponent>();
        battlePlayer.Team = prevColor == TeamColor.Red ? teamHandler.BlueTeam : teamHandler.RedTeam;
        return Task.CompletedTask;
    }
}
