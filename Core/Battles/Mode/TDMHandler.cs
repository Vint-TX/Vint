using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battles.Mode;

public class TDMHandler(
    Battle battle
) : TeamHandler(battle) {
    TeamColor LastDominationTeam { get; set; } = TeamColor.None;
    protected override List<SpawnPoint> RedSpawnPoints { get; } = battle.MapInfo.SpawnPoints.TeamDeathmatch!.Value.RedTeam.ToList();
    protected override List<SpawnPoint> BlueSpawnPoints { get; } = battle.MapInfo.SpawnPoints.TeamDeathmatch!.Value.BlueTeam.ToList();

    public override void Tick() { }

    public override TeamColor GetDominatedTeam() {
        const int dominationDiff = 30;
        const int restoreDominationDiff = 26;

        int redScore = RedTeam.GetComponent<TeamScoreComponent>().Score;
        int blueScore = BlueTeam.GetComponent<TeamScoreComponent>().Score;
        int diff = Math.Abs(redScore - blueScore);

        LastDominationTeam = diff switch {
            >= dominationDiff => redScore > blueScore ? TeamColor.Blue : TeamColor.Red,
            <= restoreDominationDiff => TeamColor.None,
            _ => LastDominationTeam
        };

        return LastDominationTeam;
    }

    public override Task OnFinished() => Task.CompletedTask;

    public override int CalculateReputationDelta(BattlePlayer player) => player.TeamBattleResult switch { // todo calculate by K/D
        TeamBattleResult.Win => player.PlayerConnection.Player.MaxReputationDelta,
        TeamBattleResult.Draw => 0,
        TeamBattleResult.Defeat => player.PlayerConnection.Player.MinReputationDelta,
        _ => -99999
    };
}
