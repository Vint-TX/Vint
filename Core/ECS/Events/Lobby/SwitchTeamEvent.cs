using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1499172594697)]
public class SwitchTeamEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby)
            return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (battle.ModeHandler is not TeamHandler teamHandler)
            return;

        UserLimitComponent userLimitComponent = battle.LobbyEntity.GetComponent<UserLimitComponent>();
        TeamColor prevColor = battlePlayer.TeamColor;

        int newTeamPlayersCount = prevColor == TeamColor.Red
            ? teamHandler.BluePlayers.Count()
            : teamHandler.RedPlayers.Count();

        if (newTeamPlayersCount >= userLimitComponent.TeamLimit)
            return;

        await connection.UserContainer.Entity.RemoveComponent<TeamColorComponent>();

        battlePlayer.Team = prevColor == TeamColor.Red
            ? teamHandler.BlueTeam
            : teamHandler.RedTeam;
    }
}
