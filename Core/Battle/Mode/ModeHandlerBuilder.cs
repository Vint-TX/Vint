using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Mode.Solo.Impl;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Battle.Mode;

namespace Vint.Core.Battle.Mode;

public class ModeHandlerBuilder(
    LobbyBase lobby
) {
    BattleProperties Properties => lobby.Properties;
    IEntity LobbyEntity => lobby.Entity;

    public ModeHandler BuildModeHandler(Round round) => Properties.BattleMode switch {
        BattleMode.DM => new DMHandler(round, BuildModeEntityFactory(round)),
        BattleMode.TDM => new TDMHandler(round, BuildModeEntityFactory(round), CreateTeamData),
        BattleMode.CTF => new CTFHandler(round, BuildModeEntityFactory(round), CreateTeamData),
        BattleMode.CP => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException()
    };

    public IEntity BuildModeEntity(Round round) => Properties.BattleMode switch {
        BattleMode.DM => new DMTemplate().Create(Properties, LobbyEntity, round.Entity, round.StartTime),
        BattleMode.TDM => new TDMTemplate().Create(Properties, LobbyEntity, round.Entity, round.StartTime),
        BattleMode.CTF => new CTFTemplate().Create(Properties, LobbyEntity, round.Entity, round.StartTime),
        BattleMode.CP => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException()
    };

    Func<IEntity> BuildModeEntityFactory(Round round) => () => BuildModeEntity(round);

    TeamData CreateTeamData(TeamColor teamColor, Round round, Func<MapSpawnPointInfo, TeamSpawnPointList> spawnPointListFactory) {
        IEntity entity = lobby.TeamHandler.ColorToEntity[teamColor]!;
        IList<SpawnPoint> spawnPoints = spawnPointListFactory(Properties.MapInfo.SpawnPoints).GetFor(teamColor);

        return new TeamData(teamColor, entity, spawnPoints, ModeHandler.GetRandomSpawnPoint, () => round.Tankers);
    }
}

public delegate TeamData CreateTeamData(TeamColor teamColor, Round round, Func<MapSpawnPointInfo, TeamSpawnPointList> spawnPointListFactory);
