using Vint.Core.Battle.Mode.Solo.Impl;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battle.Results;

public class BattleResultForClient {
    BattleResultForClient(Round round, bool isSpectator, PersonalBattleResultForClient? personalBattleResult) {
        BattleProperties properties = round.Properties;

        Spectator = isSpectator;
        BattleId = round.Entity.Id;
        Custom = properties.Type == BattleType.Custom;
        MapId = properties.MapInfo.Id;
        BattleMode = properties.BattleMode;
        MatchMakingModeType = properties.Type;

        switch (round.ModeHandler) {
            case TeamHandler teamHandler:
                TeamData redTeam = teamHandler.RedTeam;
                TeamData blueTeam = teamHandler.BlueTeam;

                RedTeamScore = redTeam.Score;
                BlueTeamScore = blueTeam.Score;

                RedUsers = redTeam.Players
                    .Select(player => player.Tank.Result)
                    .OrderByDescending(userResult => userResult.ScoreWithoutPremium)
                    .ToList();

                BlueUsers = blueTeam.Players
                    .Select(player => player.Tank.Result)
                    .OrderByDescending(userResult => userResult.ScoreWithoutPremium)
                    .ToList();

                break;

            case DMHandler:
                DmUsers = round.Tankers
                    .Select(tanker => tanker.Tank.Result)
                    .OrderByDescending(userResult => userResult.ScoreWithoutPremium)
                    .ToList();
                break;
        }

        PersonalResult = personalBattleResult;
    }

    public bool Custom { get; }
    public bool Spectator { get; }
    public long BattleId { get; }
    public long MapId { get; }
    public int RedTeamScore { get; }
    public int BlueTeamScore { get; }
    public List<UserResult> RedUsers { get; } = [];
    public List<UserResult> BlueUsers { get; } = [];
    public List<UserResult> DmUsers { get; } = [];
    public BattleMode BattleMode { get; }
    public BattleType MatchMakingModeType { get; }
    public PersonalBattleResultForClient? PersonalResult { get; }

    public int DmScore => 0; // not used

    public static BattleResultForClient CreateForSpectator(Round round) =>
        new(round, true, null);

    public static BattleResultForClient CreateForTanker(Round round, PersonalBattleResultForClient personalResult) =>
        new(round, false, personalResult);
}
