using Vint.Core.Battles.Player;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.Battles.Results;

public class UserResult {
    public UserResult(BattlePlayer battlePlayer) {
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        Database.Models.Player player = connection.Player;
        Battle battle = battlePlayer.Battle;
        Preset preset = player.CurrentPreset;
        BattleTank battleTank = battlePlayer.Tank!;
        IEntity roundUser = battleTank.RoundUser;
        RoundUserStatisticsComponent statisticsComponent = roundUser.GetComponent<RoundUserStatisticsComponent>();
        BattleTankStatistics tankStatistics = battleTank.Statistics;
        int battleUserScoreWithBonus = battlePlayer.GetBattleUserScoreWithBonus();

        Username = player.Username;
        UserId = player.Id;
        BattleUserId = battlePlayer.BattleUser.Id;
        AvatarId = connection.User.GetComponent<UserAvatarComponent>().Id;
        Rank = player.Rank;
        ReputationInBattle = player.Reputation;
        EnterTime = battleTank.BattleEnterTime.ToUnixTimeMilliseconds();

        Place = statisticsComponent.Place;
        Kills = statisticsComponent.Kills;
        KillAssists = statisticsComponent.KillAssists;
        KillStrike = tankStatistics.KillStrike;
        Deaths = statisticsComponent.Deaths;
        Damage = (int)battleTank.DealtDamage;

        ScoreWithoutPremium = statisticsComponent.ScoreWithoutBonuses;
        Score = battlePlayer.GetScoreWithBonus(ScoreWithoutPremium);
        ScoreToExperience = battleUserScoreWithBonus;
        RankExpDelta = battleUserScoreWithBonus;
        ItemsExpDelta = battleUserScoreWithBonus;

        Flags = tankStatistics.Flags;
        FlagAssists = tankStatistics.FlagAssists;
        FlagReturns = tankStatistics.FlagReturns;
        BonusesTaken = tankStatistics.BonusesTaken;

        WeaponId = preset.Weapon.Id;
        HullId = preset.Hull.Id;
        PaintId = preset.Paint.Id;
        CoatingId = preset.Cover.Id;
        HullSkinId = preset.HullSkin.Id;
        WeaponSkinId = preset.WeaponSkin.Id;

        UnfairMatching = battle.IsUnfair();
        League = player.LeagueEntity;
        Modules = preset.Modules.Select(pModule => new ModuleInfo {
            ModuleId = pModule.Entity.Id,
            UpgradeLevel = pModule.Entity.GetUserModule(connection).GetComponent<ModuleUpgradeLevelComponent>().Level
        }).ToList();
    }

    [ProtocolName("Uid")] public string Username { get; }
    public long UserId { get; }
    public long BattleUserId { get; }
    public string AvatarId { get; }
    public int Rank { get; }
    public double ReputationInBattle { get; }
    public long EnterTime { get; }
    public int Place { get; }
    public int Kills { get; }
    public int KillAssists { get; }
    public int KillStrike { get; }
    public int Deaths { get; }
    public int Damage { get; }
    public int Score { get; }
    public int ScoreWithoutPremium { get; }
    public int ScoreToExperience { get; }
    public int RankExpDelta { get; }
    public int ItemsExpDelta { get; }
    public int Flags { get; }
    public int FlagAssists { get; }
    public int FlagReturns { get; }
    public long WeaponId { get; }
    public long HullId { get; }
    public long PaintId { get; }
    public long CoatingId { get; }
    public long HullSkinId { get; }
    public long WeaponSkinId { get; }
    public int BonusesTaken { get; }
    public bool UnfairMatching { get; }
    public bool Deserted { get; set; } // todo
    public IEntity League { get; }
    public List<ModuleInfo> Modules { get; }
}
