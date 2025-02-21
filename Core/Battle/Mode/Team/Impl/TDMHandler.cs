using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battle.Mode.Team.Impl;

public class TDMHandler(
    Round round,
    Func<IEntity> entityFactory,
    CreateTeamData createTeamData
) : TeamHandler(round, createTeamData, info => info.TeamDeathmatch!.Value) {
    TeamColor LastDominatedTeam { get; set; } = TeamColor.None;
    public override IEntity Entity { get; } = entityFactory();

    public override TeamColor GetDominatedTeamColor() {
        const int dominationThreshold = 30;
        const int restoreThreshold = 26;

        int redScore = RedTeam.Score;
        int blueScore = BlueTeam.Score;
        int scoreDiff = Math.Abs(redScore - blueScore);
        TeamColor dominatedTeam = redScore > blueScore ? TeamColor.Blue : TeamColor.Red;

        LastDominatedTeam = scoreDiff switch {
            >= dominationThreshold => dominatedTeam,
            <= restoreThreshold => TeamColor.None,
            _ => LastDominatedTeam
        };

        return LastDominatedTeam;
    }

    public override int CalculateReputationDelta(Tanker tanker) => tanker.TeamResult switch { // todo calculate by K/D
        TeamBattleResult.Win => tanker.Connection.Player.MaxReputationDelta,
        TeamBattleResult.Draw => 0,
        TeamBattleResult.Defeat => tanker.Connection.Player.MinReputationDelta,
        _ => throw new ArgumentOutOfRangeException()
    };
}
