using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.Battles.Results;

public class UserResult(
    BattlePlayer battlePlayer
) {
    [ProtocolIgnore] IPlayerConnection Connection => battlePlayer.PlayerConnection;
    [ProtocolIgnore] Database.Models.Player Player => Connection.Player;
    [ProtocolIgnore] Battle Battle => battlePlayer.Battle;
    [ProtocolIgnore] Preset Preset => Player.CurrentPreset;
    [ProtocolIgnore] BattleTank BattleTank => battlePlayer.Tank!;
    [ProtocolIgnore] IEntity RoundUser => BattleTank.RoundUser;

    [ProtocolName("Uid")] public string Username => Player.Username;
    public long UserId => Player.Id;
    public long BattleUserId => battlePlayer.BattleUser.Id;
    public string AvatarId => Connection.User.GetComponent<UserAvatarComponent>().Id;
    public int Rank => Player.Rank;
    public double ReputationInBattle => Player.Reputation;
    public long EnterTime => BattleTank.BattleEnterTime.ToUnixTimeMilliseconds();
    public int Place => RoundUser.GetComponent<RoundUserStatisticsComponent>().Place;
    public int Kills => RoundUser.GetComponent<RoundUserStatisticsComponent>().Kills;
    public int KillAssists => RoundUser.GetComponent<RoundUserStatisticsComponent>().KillAssists;
    public int KillStrike { get; set; }
    public int Deaths => RoundUser.GetComponent<RoundUserStatisticsComponent>().Deaths;
    public int Damage => (int)BattleTank.DealtDamage;
    public int Score => battlePlayer.GetScoreWithBonus(ScoreWithoutPremium);
    public int ScoreWithoutPremium => RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses;
    public int ScoreToExperience => battlePlayer.GetBattleUserScoreWithBonus();
    public int RankExpDelta => battlePlayer.GetBattleUserScoreWithBonus();
    public int ItemsExpDelta => battlePlayer.GetBattleUserScoreWithBonus();
    public int Flags { get; set; }
    public int FlagAssists { get; set; }
    public int FlagReturns { get; set; }
    public long WeaponId => Preset.Weapon.Id;
    public long HullId => Preset.Hull.Id;
    public long PaintId => Preset.Paint.Id;
    public long CoatingId => Preset.Cover.Id;
    public long HullSkinId => Preset.HullSkin.Id;
    public long WeaponSkinId => Preset.WeaponSkin.Id;
    public int BonusesTaken { get; set; }
    public bool UnfairMatching => Battle.ModeHandler is TeamHandler teamHandler && teamHandler.BluePlayers.Count() != teamHandler.RedPlayers.Count();
    public bool Deserted { get; set; } // todo
    public IEntity League => Player.LeagueEntity;
    public List<ModuleInfo> Modules => Preset.Modules
        .Select(pModule => new ModuleInfo {
            ModuleId = pModule.Entity.Id,
            UpgradeLevel = pModule.Entity.GetUserModule(Connection).GetComponent<ModuleUpgradeLevelComponent>().Level
        }).ToList();
}