using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Results;

public class UserResult {
    public UserResult(Tanker tanker) {
        IPlayerConnection connection = tanker.Connection;
        Database.Models.Player player = connection.Player;
        Round round = tanker.Round;
        Preset preset = player.CurrentPreset;
        BattleTank battleTank = tanker.Tank;
        IEntity roundUser = battleTank.Entities.RoundUser;
        RoundUserStatisticsComponent statisticsComponent = roundUser.GetComponent<RoundUserStatisticsComponent>();
        BattleTankStatistics tankStatistics = battleTank.Statistics;

        Username = player.Username;
        UserId = player.Id;
        AvatarId = connection.UserContainer.Entity.GetComponent<UserAvatarComponent>().Id;
        Rank = player.Rank;

        Kills = statisticsComponent.Kills;
        KillAssists =  tankStatistics.KillAssists;
        KillStrike = tankStatistics.MaxKillStrike;
        Deaths = statisticsComponent.Deaths;
        Damage = (int)tankStatistics.DealtDamage;

        ScoreWithoutPremium = statisticsComponent.ScoreWithoutBonuses;

        Flags = tankStatistics.Flags;
        FlagReturns = tankStatistics.FlagReturns;
        BonusesTaken = tankStatistics.BonusesTaken;

        WeaponId = preset.Weapon.Id;
        HullId = preset.Hull.Id;
        PaintId = preset.Paint.Id;
        CoatingId = preset.Cover.Id;
        HullSkinId = preset.HullSkin.Id;
        WeaponSkinId = preset.WeaponSkin.Id;

        UnfairMatching = round.IsUnfair();
        League = player.LeagueEntity;

        Modules = preset
            .Modules
            .Select(pModule => new ModuleInfo {
                ModuleId = pModule.Entity.Id,
                UpgradeLevel = pModule.Entity
                    .GetUserModule(connection)
                    .GetComponent<ModuleUpgradeLevelComponent>()
                    .Level
            })
            .ToList();

        TeamResult = CalculateTeamResult(tanker);
    }

    [ProtocolName("Uid")] public string Username { get; }
    public long UserId { get; }
    public string AvatarId { get; }
    public int Rank { get; }
    public int Kills { get; }
    public int KillAssists { get; }
    public int KillStrike { get; }
    public int Deaths { get; }
    public int Damage { get; }
    public int ScoreWithoutPremium { get; }
    public int Flags { get; }
    public int FlagReturns { get; }
    public long WeaponId { get; }
    public long HullId { get; }
    public long PaintId { get; }
    public long CoatingId { get; }
    public long HullSkinId { get; }
    public long WeaponSkinId { get; }
    public int BonusesTaken { get; }
    public bool UnfairMatching { get; }
    public IEntity League { get; }
    public List<ModuleInfo> Modules { get; }

    [ProtocolIgnore] public TeamBattleResult TeamResult { get; }

    static TeamBattleResult CalculateTeamResult(Tanker tanker) {
        if (tanker.Round.ModeHandler is not TeamHandler teamHandler)
            return TeamBattleResult.Draw;

        int scoreDiff = teamHandler.RedTeam.Score - teamHandler.BlueTeam.Score;

        if (scoreDiff == 0)
            return TeamBattleResult.Draw;

        TeamColor winningTeam = scoreDiff > 0 ? TeamColor.Red : TeamColor.Blue;

        return tanker.TeamColor == winningTeam
            ? TeamBattleResult.Win
            : TeamBattleResult.Defeat;
    }
}
