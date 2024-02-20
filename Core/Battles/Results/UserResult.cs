using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.Battles.Results;

public class UserResult(
    BattlePlayer battlePlayer
) {
    [ProtocolName("uid")] public string Username => battlePlayer.PlayerConnection.Player.Username;
    public long UserId => battlePlayer.PlayerConnection.Player.Id;
    public long BattleUserId => battlePlayer.BattleUser.Id;
    public string AvatarId => battlePlayer.PlayerConnection.User.GetComponent<UserAvatarComponent>().Id;
    public int Rank => battlePlayer.PlayerConnection.Player.Rank;
    public double ReputationInBattle => battlePlayer.PlayerConnection.Player.Reputation;
    public long EnterTime => battlePlayer.Tank!.BattleEnterTime.ToUnixTimeMilliseconds();
    public int Place => battlePlayer.Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>().Place;
    public int Kills => battlePlayer.Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>().Kills;
    public int KillAssists => battlePlayer.Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>().KillAssists;
    public int KillStrike { get; set; }
    public int Deaths => battlePlayer.Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>().Deaths;
    public int Damage => (int)battlePlayer.Tank!.DealtDamage;
    public int Score => battlePlayer.GetScoreWithBonus(ScoreWithoutPremium);
    public int ScoreWithoutPremium => battlePlayer.Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses;
    public int ScoreToExperience => battlePlayer.GetBattleUserScoreWithBonus();
    public int RankExpDelta => battlePlayer.GetBattleUserScoreWithBonus();
    public int ItemsExpDelta => battlePlayer.GetBattleUserScoreWithBonus();
    public int Flags { get; set; }
    public int FlagAssists { get; set; }
    public int FlagReturns { get; set; }
    public long WeaponId => battlePlayer.PlayerConnection.Player.CurrentPreset.Weapon.Id;
    public long HullId => battlePlayer.PlayerConnection.Player.CurrentPreset.Hull.Id;
    public long PaintId => battlePlayer.PlayerConnection.Player.CurrentPreset.Paint.Id;
    public long CoatingId => battlePlayer.PlayerConnection.Player.CurrentPreset.Cover.Id;
    public long HullSkinId => battlePlayer.PlayerConnection.Player.CurrentPreset.HullSkin.Id;
    public long WeaponSkinId => battlePlayer.PlayerConnection.Player.CurrentPreset.WeaponSkin.Id;
    public int BonusesTaken { get; set; }
    public bool UnfairMatching { get; set; } // todo
    public bool Deserted { get; set; } // todo
    public IEntity League => battlePlayer.PlayerConnection.Player.LeagueEntity;
    public List<ModuleInfo> Modules { get; set; } = []; // todo modules
}