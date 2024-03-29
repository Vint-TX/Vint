using LinqToDB;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Results;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Player;

public class BattlePlayer {
    IEntity? _team;
    TeamColor _teamColor = TeamColor.None;

    public BattlePlayer(IPlayerConnection playerConnection, Battle battle, IEntity? team, bool isSpectator) {
        PlayerConnection = playerConnection;
        Team = team;
        Battle = battle;
        IsSpectator = isSpectator;

        if (IsSpectator)
            BattleUser = new BattleUserTemplate().CreateAsSpectator(PlayerConnection.User, Battle.Entity);
    }

    public IPlayerConnection PlayerConnection { get; }

    public IEntity? Team {
        get => _team;
        set {
            _team = value;
            TeamColor = _team?.GetComponent<TeamColorComponent>().TeamColor ?? TeamColor.None;
        }
    }

    public IEntity BattleUser { get; set; } = null!;

    public Battle Battle { get; }
    public BattleTank? Tank { get; set; }

    public TeamColor TeamColor {
        get => _teamColor;
        private set {
            _teamColor = value;

            if (PlayerConnection.User.HasComponent<TeamColorComponent>())
                PlayerConnection.User.ChangeComponent<TeamColorComponent>(component => component.TeamColor = _teamColor);
            else
                PlayerConnection.User.AddComponent(new TeamColorComponent(_teamColor));
        }
    }

    public bool IsSpectator { get; }
    public bool InBattleAsTank => Tank != null;
    public bool InBattle { get; set; }
    public bool IsPaused { get; set; }
    public bool IsKicked { get; set; }
    bool Reported { get; set; }

    public DateTimeOffset BattleJoinTime { get; set; } = DateTimeOffset.UtcNow.AddSeconds(20);
    public DateTimeOffset? KickTime { get; set; }

    public TeamBattleResult TeamBattleResult {
        get {
            switch (Battle.ModeHandler) {
                case TeamHandler teamHandler: {
                    TeamColor winningTeam;
                    int redScore = teamHandler.RedTeam.GetComponent<TeamScoreComponent>().Score;
                    int blueScore = teamHandler.BlueTeam.GetComponent<TeamScoreComponent>().Score;

                    if (redScore > blueScore) winningTeam = TeamColor.Red;
                    else if (blueScore > redScore) winningTeam = TeamColor.Blue;
                    else return TeamBattleResult.Draw;

                    return TeamColor == winningTeam ? TeamBattleResult.Win : TeamBattleResult.Defeat;
                }

                default: return TeamBattleResult.Draw;
            }
        }
    }

    public void Init() {
        PlayerConnection.Share(Battle.Entity, Battle.RoundEntity, Battle.BattleChatEntity);
        Battle.BonusProcessor?.ShareEntities(PlayerConnection);

        foreach (Effect effect in Battle.Players
                     .Where(battlePlayer => battlePlayer.InBattleAsTank)
                     .SelectMany(battlePlayer => battlePlayer.Tank!.Effects))
            effect.Share(this);

        PlayerConnection.User.AddComponentFrom<BattleGroupComponent>(Battle.Entity);
        Battle.ModeHandler.PlayerEntered(this);

        if (IsSpectator) {
            PlayerConnection.Share(BattleUser);
            InBattle = true;
        } else {
            Tank = new BattleTank(this);

            // todo modules

            InBattle = true;

            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                player.PlayerConnection.Share(Tank.Entities); // Share this player entities to players in battle

            Battle.ModeHandler.SortPlayers();
        }

        PlayerConnection.Share(Battle.Players
            .Where(player => player != this && player.InBattleAsTank)
            .SelectMany(player => player.Tank!.Entities));
    }

    public void OnBattleEnded() {
        Database.Models.Player player = PlayerConnection.Player;

        if (IsSpectator) {
            BattleResultForClient specResult = new(Battle, IsSpectator, null);
            PlayerConnection.Send(new BattleResultForClientEvent(specResult), PlayerConnection.User);
            return;
        }

        Preset preset = player.CurrentPreset;
        IEntity previousLeague = player.LeagueEntity;
        int reputationDelta;
        using DbConnection db = new();

        db.BeginTransaction();
        db.SeasonStatistics
            .Where(stats => stats.PlayerId == player.Id &&
                            stats.SeasonNumber == ConfigManager.SeasonNumber)
            .Set(stats => stats.BattlesPlayed, stats => stats.BattlesPlayed + 1)
            .Update();

        db.Hulls
            .Where(hull => hull.PlayerId == player.Id &&
                           hull.Id == preset.Hull.Id)
            .Set(hull => hull.BattlesPlayed, hull => hull.BattlesPlayed + 1)
            .Update();

        db.Weapons
            .Where(weapon => weapon.PlayerId == player.Id &&
                             weapon.Id == preset.Weapon.Id)
            .Set(weapon => weapon.BattlesPlayed, weapon => weapon.BattlesPlayed + 1)
            .Update();

        if (Battle.TypeHandler is not MatchmakingHandler) {
            db.Statistics
                .Where(stats => stats.PlayerId == player.Id)
                .Set(stats => stats.AllBattlesParticipated, stats => stats.AllBattlesParticipated + 1)
                .Set(stats => stats.AllCustomBattlesParticipated, stats => stats.AllCustomBattlesParticipated + 1)
                .Update();

            db.CommitTransaction();
            reputationDelta = 0;
        } else {
            db.Statistics
                .Where(stats => stats.PlayerId == player.Id)
                .Set(stats => stats.AllBattlesParticipated, stats => stats.AllBattlesParticipated + 1)
                .Set(stats => stats.BattlesParticipated, stats => stats.BattlesParticipated + 1)
                .Set(stats => stats.Defeats, stats => stats.Defeats + (TeamBattleResult == TeamBattleResult.Defeat ? 1 : 0))
                .Set(stats => stats.Victories, stats => stats.Victories + (TeamBattleResult == TeamBattleResult.Win ? 1 : 0))
                .Update();

            db.CommitTransaction();

            if (player.DesertedBattlesCount == 0)
                PlayerConnection.BattleSeries++;

            int score = GetBattleUserScoreWithBonus();

            Leveling.UpdateItemXp(preset.Hull, PlayerConnection, score);
            Leveling.UpdateItemXp(preset.Weapon, PlayerConnection, score);

            reputationDelta = Battle.ModeHandler.CalculateReputationDelta(this);
            PlayerConnection.ChangeReputation(reputationDelta);
            PlayerConnection.ChangeGameplayChestScore(score);
        }

        PersonalBattleResultForClient personalBattleResult = new(PlayerConnection, previousLeague, reputationDelta);
        BattleResultForClient battleResult = new(Battle, IsSpectator, personalBattleResult);

        PlayerConnection.Send(new BattleResultForClientEvent(battleResult), PlayerConnection.User);
    }

    public void OnAntiCheatSuspected() {
        if (Reported) return;

        PlayerConnection.Server.DiscordBot?.SendReport($"{PlayerConnection.Player.Username} is suspected to be cheating.", "Server");
        Reported = true;
    }

    public float GetBattleSeriesMultiplier() {
        float[] battleSeriesMultiplier = [1f, 1.05f, 1.10f, 1.15f, 1.2f, 1.25f];
        int series = Math.Clamp(PlayerConnection.BattleSeries, 0, battleSeriesMultiplier.Length - 1);
        return battleSeriesMultiplier[series];
    }

    public int GetScoreWithBonus(int score) =>
        (int)Math.Round(score * GetBattleSeriesMultiplier() + score * (PlayerConnection.Player.IsPremium ? 0.5 : 0));

    public int GetBattleUserScoreWithBonus() {
        int scoreWithoutBonuses = Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses;
        int scoreWithBonus = GetScoreWithBonus(scoreWithoutBonuses);
        float battleSeriesMultiplier = GetBattleSeriesMultiplier() / 100;

        return (int)Math.Round(scoreWithBonus + scoreWithoutBonuses * battleSeriesMultiplier);
    }

    public void RankUp() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
            battlePlayer.PlayerConnection.Send(new UpdateRankEvent(), PlayerConnection.User);
    }

    public void Tick() => Tank?.Tick();

    public override int GetHashCode() => PlayerConnection.GetHashCode();
}