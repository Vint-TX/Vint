using System.Diagnostics.CodeAnalysis;
using LinqToDB;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Server.Battle.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Player;

public class Tanker : BattlePlayer {
    public Tanker(Round round, IPlayerConnection connection, IEntity? team) : base(round, connection) {
        Team = team;
        TeamColor = Team?.GetComponent<TeamColorComponent>().TeamColor ?? TeamColor.None;
        BattleUser = new BattleUserTemplate().CreateAsTank(connection.UserContainer.Entity, round.Entity, team);
        Tank = new BattleTank(this);
    }

    public override IEntity BattleUser { get; }
    public BattleTank Tank { get; }
    public IEntity? Team { get; }
    public TeamColor TeamColor { get; }
    public TeamBattleResult TeamResult => Tank.Result.TeamResult;

    public IdleKickConfigComponent IdleKickConfig { get; } = ConfigManager.GetComponent<IdleKickConfigComponent>("battle/battleuser");
    public SelfDestructionConfigComponent SelfDestructionConfig { get; } = ConfigManager.GetComponent<SelfDestructionConfigComponent>("battle/battleuser");
    public ReservationConfigComponent ReservationConfig { get; } = ConfigManager.GetComponent<ReservationConfigComponent>("battle/battleuser");

    [MemberNotNullWhen(true, nameof(KickTime))]
    public bool IsPaused {
        get;
        set {
            field = value;

            if (field) KickTime = DateTimeOffset.UtcNow.AddSeconds(IdleKickConfig.IdleKickTimeSec);
            else KickTime = null;
        }
    }

    [MemberNotNullWhen(true, nameof(ReservationUntilTime))]
    public bool IsReserved {
        get;
        set {
            field = value;

            if (field) ReservationUntilTime = DateTimeOffset.UtcNow.AddSeconds(ReservationConfig.ReservationDurationTimeSec);
            else ReservationUntilTime = null;
        }
    }

    public DateTimeOffset? KickTime { get; private set; }
    public DateTimeOffset? ReservationUntilTime { get; private set; }

    bool Reported { get; set; }

    public float ScoreMultiplier {
        get {
            if (field is not default(float))
                return field;

            float seriesMultiplier = GetBattleSeriesMultiplier();
            float premiumMultiplier = Connection.Player.IsPremium ? .5f : 0;

            field = 1 + seriesMultiplier + premiumMultiplier;
            return field;
        }
    }

    public override async Task Init() {
        await base.Init();

        await Tank.Init();
        await Round.Players.Share(Tank.Entities);
    }

    public override async Task DeInit() {
        await Round.Players.Unshare(Tank.Entities);
        await Tank.DeInit();

        await base.DeInit();
    }

    public override async Task OnRoundEnded(bool hasEnemies, QuestManager questManager) {
        await using DbConnection db = new();

        Database.Models.Player player = Connection.Player;
        Preset preset = player.CurrentPreset;
        IEntity previousLeague = player.LeagueEntity;
        int reputationDelta;

        await db.BeginTransactionAsync();

        await db.SeasonStatistics
            .Where(stats => stats.PlayerId == player.Id && stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber)
            .Set(stats => stats.BattlesPlayed, stats => stats.BattlesPlayed + 1)
            .UpdateAsync();

        await db.Hulls
            .Where(hull => hull.PlayerId == player.Id && hull.Id == preset.Hull.Id)
            .Set(hull => hull.BattlesPlayed, hull => hull.BattlesPlayed + 1)
            .UpdateAsync();

        await db.Weapons
            .Where(weapon => weapon.PlayerId == player.Id && weapon.Id == preset.Weapon.Id)
            .Set(weapon => weapon.BattlesPlayed, weapon => weapon.BattlesPlayed + 1)
            .UpdateAsync();

        if (Round.Properties.Type != BattleType.Rating) {
            await db.Statistics
                .Where(stats => stats.PlayerId == player.Id)
                .Set(stats => stats.AllBattlesParticipated, stats => stats.AllBattlesParticipated + 1)
                .Set(stats => stats.AllCustomBattlesParticipated, stats => stats.AllCustomBattlesParticipated + 1)
                .UpdateAsync();

            await db.CommitTransactionAsync();
            reputationDelta = 0;
        } else {
            await db.Statistics
                .Where(stats => stats.PlayerId == player.Id)
                .Set(stats => stats.AllBattlesParticipated, stats => stats.AllBattlesParticipated + 1)
                .Set(stats => stats.BattlesParticipated, stats => stats.BattlesParticipated + 1)
                .Set(stats => stats.Defeats, stats => stats.Defeats + (TeamResult == TeamBattleResult.Defeat ? 1 : 0))
                .Set(stats => stats.Victories, stats => stats.Victories + (TeamResult == TeamBattleResult.Win ? 1 : 0))
                .UpdateAsync();

            await db.CommitTransactionAsync();

            if (!player.IsDeserter)
                Connection.BattleSeries++;

            int score = GetBattleUserScoreWithBonus();

            await Leveling.UpdateItemXp(preset.Hull, Connection, score);
            await Leveling.UpdateItemXp(preset.Weapon, Connection, score);

            reputationDelta = Round.ModeHandler.CalculateReputationDelta(this);

            if (Tank.Result.UnfairMatching)
                reputationDelta /= 2;

            await Connection.ChangeReputation(reputationDelta);
            await Connection.ChangeGameplayChestScore(score);

            if (hasEnemies && Round.Properties.Type == BattleType.Rating)
                await questManager.BattleFinished(this);
        }

        PersonalBattleResultForClient personalBattleResult = new(previousLeague, reputationDelta);
        await personalBattleResult.Init(Connection);

        BattleResultForClient battleResult = BattleResultForClient.CreateForTanker(Round, personalBattleResult);
        await Connection.Send(new BattleResultForClientEvent(battleResult), Connection.UserContainer.Entity);
    }

    public async Task OnAntiCheatSuspected(DiscordBot? discordBot) {
        if (Reported) return;

        if (discordBot != null)
            await discordBot.SendReport($"{Connection.Player.Username} is suspected to be cheating.", "Server");

        Reported = true;
    }

    public int GetScoreWithBonus(int score) =>
        (int)Math.Round(score * ScoreMultiplier);

    public int GetBattleUserScoreWithBonus() {
        int score = Tank.Entities.RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses;
        return GetScoreWithBonus(score);
    }

    public override async Task Tick(TimeSpan deltaTime) =>
        await Tank.Tick(deltaTime);

    float GetBattleSeriesMultiplier() {
        float[] battleSeriesMultiplier = [0f, .05f, .1f, .15f, .2f, .25f];
        int series = Math.Clamp(Connection.BattleSeries, 0, battleSeriesMultiplier.Length - 1);
        return battleSeriesMultiplier[series];
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) { // todo dispose entities
            Tank.Dispose();
        }
    }
}
