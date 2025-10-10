using System.Collections.Concurrent;
using LinqToDB;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player.User.Templates;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Results.Events;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Quests;
using Vint.Core.Server.Game.Connection;
using Vint.Core.User.Components;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Player;

public sealed class HumanTanker : Tanker {
    public HumanTanker(Round round, IPlayerConnection connection, IEntity? team) : base(round, connection) {
        Team = team;
        TeamColor = Team?.GetComponent<TeamColorComponent>().TeamColor ?? TeamColor.None;
        BattleUser = new BattleUserTemplate().CreateAsTank(connection.UserContainer.Entity, round.Entity, team);
        Tank = new BattleTank(this);
    }

    public ConcurrentDictionary<int, BotTanker> ControlledBots { get; } = [];

    public override IEntity BattleUser { get; }
    public override BattleTank Tank { get; }
    public override IEntity? Team { get; }
    public override TeamColor TeamColor { get; }
    public override TeamBattleResult TeamResult => Tank.Result.TeamResult;

    public override bool Reported { get; set; }

    public override float ScoreMultiplier {
        get {
            if (field is not default(float))
                return field;

            float seriesMultiplier = GetBattleSeriesMultiplier();
            float premiumMultiplier = Connection.Player.HasPremiumBoost ? .5f : 0;

            field = 1 + seriesMultiplier + premiumMultiplier;
            return field;
        }
    }

    public override async Task OnRoundEnded(bool hasEnemies, QuestManager questManager) {
        Database.Models.Player player = Connection.Player;
        Preset preset = player.CurrentPreset;
        IEntity previousLeague = player.LeagueEntity;
        int reputationDelta;

        await using (DbConnection db = new()) {
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

            if (Round.Properties.GetValue(BattleProperty.Type) != BattleType.Rating) {
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

                if (hasEnemies && Round.Properties.GetValue(BattleProperty.Type) == BattleType.Rating)
                    await questManager.BattleFinished(this);
            }

            Statistics stats = await db.Statistics.FirstAsync(stats => stats.PlayerId == player.Id);
            await Connection.UserContainer.Entity.ChangeComponent<UserStatisticsComponent>(component => component.Statistics = stats.CollectClientSide());
        }

        PersonalBattleResultForClient personalBattleResult = new(previousLeague, reputationDelta);
        await personalBattleResult.Init(Connection);

        BattleResultForClient battleResult = BattleResultForClient.CreateForTanker(Round, personalBattleResult);
        await Connection.Send(new BattleResultForClientEvent(battleResult), Connection.UserContainer.Entity);
    }

    public override int GetScoreWithBonus(int score) =>
        (int)Math.Round(score * ScoreMultiplier);

    public override async Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken) =>
        await Tank.Tick(deltaTime, cancellationToken);

    float GetBattleSeriesMultiplier() {
        float[] battleSeriesMultiplier = [0f, .05f, .1f, .15f, .2f, .25f];
        int series = Math.Clamp(Connection.BattleSeries, 0, battleSeriesMultiplier.Length - 1);
        return battleSeriesMultiplier[series];
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            Tank.Dispose();
        }
    }
}
