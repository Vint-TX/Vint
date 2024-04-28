using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battles.Results;

public class BattleResultForClient {
    public BattleResultForClient(Battle battle, bool isSpectator, PersonalBattleResultForClient? personalBattleResult) {
        Spectator = isSpectator;
        Custom = battle.TypeHandler is CustomHandler;
        BattleId = battle.Id;
        MapId = battle.MapInfo.Id;
        BattleMode = battle.Properties.BattleMode;
        MatchMakingModeType = battle.TypeHandler is ArcadeHandler ? BattleType.Arcade : BattleType.Rating;

        switch (battle.ModeHandler) {
            case TeamHandler teamHandler:
                RedTeamScore = teamHandler.RedTeam.GetComponent<TeamScoreComponent>().Score;
                BlueTeamScore = teamHandler.BlueTeam.GetComponent<TeamScoreComponent>().Score;

                RedUsers = teamHandler.RedPlayers
                    .Select(player => player.Tank!.Result)
                    .OrderByDescending(userResult => userResult.ScoreWithoutPremium)
                    .ToList();

                BlueUsers = teamHandler.BluePlayers
                    .Select(player => player.Tank!.Result)
                    .OrderByDescending(userResult => userResult.ScoreWithoutPremium)
                    .ToList();
                break;

            case DMHandler:
                List<UserResult> userResults = battle.Players
                    .Where(player => player.InBattleAsTank)
                    .Select(player => player.Tank!.Result)
                    .OrderByDescending(userResult => userResult.ScoreWithoutPremium)
                    .ToList();

                DmScore = userResults.Sum(userResult => userResult.Kills);
                DmUsers = userResults;
                break;
        }

        PersonalResult = personalBattleResult;
    }

    public bool Custom { get; set; }
    public bool Spectator { get; set; }
    public long BattleId { get; set; }
    public long MapId { get; set; }
    public int RedTeamScore { get; set; }
    public int BlueTeamScore { get; set; }
    public int DmScore { get; set; }
    public List<UserResult> RedUsers { get; set; } = [];
    public List<UserResult> BlueUsers { get; set; } = [];
    public List<UserResult> DmUsers { get; set; } = [];
    public BattleMode BattleMode { get; set; }
    public BattleType MatchMakingModeType { get; set; }
    public PersonalBattleResultForClient? PersonalResult { get; set; }
}
