using LinqToDB;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Results;
using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Player;

public class BattlePlayer {
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
        get;
        set {
            field = value;

            TeamColor = field?.GetComponent<TeamColorComponent>()
                            .TeamColor ??
                        TeamColor.None;
        }
    }

    public IEntity BattleUser { get; set; } = null!;

    public Battle Battle { get; }
    public BattleTank? Tank { get; set; }

    public TeamColor TeamColor {
        get;
        private set {
            field = value;

            if (PlayerConnection.User.HasComponent<TeamColorComponent>())
                PlayerConnection.User.ChangeComponent<TeamColorComponent>(component => component.TeamColor = field);
            else
                PlayerConnection.User.AddComponent(new TeamColorComponent(field));
        }
    } = TeamColor.None;

    public bool IsSpectator { get; }
    public bool InBattleAsTank => InBattle && Tank != null;
    public bool InBattle { get; set; }
    public bool IsPaused { get; set; }
    bool Reported { get; set; }

    public DateTimeOffset BattleJoinTime { get; set; } = DateTimeOffset.UtcNow.AddSeconds(20);
    public DateTimeOffset? KickTime { get; set; }

    public TeamBattleResult TeamBattleResult {
        get {
            switch (Battle.ModeHandler) {
                case TeamHandler teamHandler: {
                    TeamColor winningTeam;

                    int redScore = teamHandler.RedTeam.GetComponent<TeamScoreComponent>()
                        .Score;

                    int blueScore = teamHandler.BlueTeam.GetComponent<TeamScoreComponent>()
                        .Score;

                    if (redScore > blueScore) winningTeam = TeamColor.Red;
                    else if (blueScore > redScore) winningTeam = TeamColor.Blue;
                    else return TeamBattleResult.Draw;

                    return TeamColor == winningTeam
                        ? TeamBattleResult.Win
                        : TeamBattleResult.Defeat;
                }

                default: return TeamBattleResult.Draw;
            }
        }
    }

    public async Task Init() {
        await PlayerConnection.Share(Battle.Entity, Battle.RoundEntity, Battle.BattleChatEntity);
        Battle.BonusProcessor?.ShareEntities(PlayerConnection);

        foreach (Effect effect in Battle
                     .Players
                     .Where(battlePlayer => battlePlayer.InBattleAsTank)
                     .SelectMany(battlePlayer => battlePlayer.Tank!.Effects))
            await effect.Share(this);

        await PlayerConnection.User.AddGroupComponent<BattleGroupComponent>(Battle.Entity);
        await PlayerConnection.User.RemoveComponentIfPresent<MatchMakingUserReadyComponent>();
        await Battle.ModeHandler.PlayerEntered(this);

        if (IsSpectator) {
            await PlayerConnection.Share(BattleUser);
            InBattle = true;
        } else {
            Tank = new BattleTank(this);

            if (!Battle.Properties.DisabledModules)
                await Tank.InitModules();

            InBattle = true;

            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                await player.PlayerConnection.Share(Tank.Entities); // Share this player entities to players in battle

            await Battle.ModeHandler.SortPlayers();
        }

        await PlayerConnection.Share(Battle
            .Players
            .Where(player => player != this && player.InBattleAsTank)
            .SelectMany(player => player.Tank!.Entities));
    }

    public async Task OnBattleEnded(bool hasEnemies, QuestManager questManager) {
        Database.Models.Player player = PlayerConnection.Player;

        if (IsSpectator) {
            BattleResultForClient specResult = new(Battle, IsSpectator, null);
            await PlayerConnection.Send(new BattleResultForClientEvent(specResult), PlayerConnection.User);
            return;
        }

        Preset preset = player.CurrentPreset;
        IEntity previousLeague = player.LeagueEntity;
        int reputationDelta;
        await using DbConnection db = new();

        await db.BeginTransactionAsync();

        await db
            .SeasonStatistics
            .Where(stats => stats.PlayerId == player.Id && stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber)
            .Set(stats => stats.BattlesPlayed, stats => stats.BattlesPlayed + 1)
            .UpdateAsync();

        await db
            .Hulls
            .Where(hull => hull.PlayerId == player.Id && hull.Id == preset.Hull.Id)
            .Set(hull => hull.BattlesPlayed, hull => hull.BattlesPlayed + 1)
            .UpdateAsync();

        await db
            .Weapons
            .Where(weapon => weapon.PlayerId == player.Id && weapon.Id == preset.Weapon.Id)
            .Set(weapon => weapon.BattlesPlayed, weapon => weapon.BattlesPlayed + 1)
            .UpdateAsync();

        if (Battle.TypeHandler is not MatchmakingHandler) {
            await db
                .Statistics
                .Where(stats => stats.PlayerId == player.Id)
                .Set(stats => stats.AllBattlesParticipated, stats => stats.AllBattlesParticipated + 1)
                .Set(stats => stats.AllCustomBattlesParticipated, stats => stats.AllCustomBattlesParticipated + 1)
                .UpdateAsync();

            await db.CommitTransactionAsync();
            reputationDelta = 0;
        } else {
            await db
                .Statistics
                .Where(stats => stats.PlayerId == player.Id)
                .Set(stats => stats.AllBattlesParticipated, stats => stats.AllBattlesParticipated + 1)
                .Set(stats => stats.BattlesParticipated, stats => stats.BattlesParticipated + 1)
                .Set(stats => stats.Defeats,
                    stats => stats.Defeats +
                             (TeamBattleResult == TeamBattleResult.Defeat
                                 ? 1
                                 : 0))
                .Set(stats => stats.Victories,
                    stats => stats.Victories +
                             (TeamBattleResult == TeamBattleResult.Win
                                 ? 1
                                 : 0))
                .UpdateAsync();

            await db.CommitTransactionAsync();

            if (!player.IsDeserter)
                PlayerConnection.BattleSeries++;

            int score = GetBattleUserScoreWithBonus();

            await Leveling.UpdateItemXp(preset.Hull, PlayerConnection, score);
            await Leveling.UpdateItemXp(preset.Weapon, PlayerConnection, score);

            reputationDelta = Battle.ModeHandler.CalculateReputationDelta(this);

            if (Tank!.Result.UnfairMatching)
                reputationDelta /= 2;

            await PlayerConnection.ChangeReputation(reputationDelta);
            await PlayerConnection.ChangeGameplayChestScore(score);

            await questManager.BattleFinished(PlayerConnection, hasEnemies);
        }

        PersonalBattleResultForClient personalBattleResult = new();
        await personalBattleResult.Init(PlayerConnection, previousLeague, reputationDelta);

        BattleResultForClient battleResult = new(Battle, IsSpectator, personalBattleResult);

        await PlayerConnection.Send(new BattleResultForClientEvent(battleResult), PlayerConnection.User);
    }

    public async Task OnAntiCheatSuspected(DiscordBot? discordBot) {
        if (Reported) return;

        if (discordBot != null)
            await discordBot.SendReport($"{PlayerConnection.Player.Username} is suspected to be cheating.", "Server");

        Reported = true;
    }

    public float GetBattleSeriesMultiplier() {
        float[] battleSeriesMultiplier = [1f, 1.05f, 1.1f, 1.15f, 1.2f, 1.25f];
        int series = Math.Clamp(PlayerConnection.BattleSeries, 0, battleSeriesMultiplier.Length - 1);
        return battleSeriesMultiplier[series];
    }

    public int GetScoreWithBonus(int score) =>
        (int)Math.Round(score * GetBattleSeriesMultiplier() +
                        score *
                        (PlayerConnection.Player.IsPremium
                            ? 0.5
                            : 0));

    public int GetBattleUserScoreWithBonus() {
        int scoreWithoutBonuses = Tank!.RoundUser.GetComponent<RoundUserStatisticsComponent>()
            .ScoreWithoutBonuses;

        int scoreWithBonus = GetScoreWithBonus(scoreWithoutBonuses);
        float battleSeriesMultiplier = GetBattleSeriesMultiplier() / 100;

        return (int)Math.Round(scoreWithBonus * battleSeriesMultiplier);
    }

    public async Task RankUp() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
            await battlePlayer.PlayerConnection.Send(new UpdateRankEvent(), PlayerConnection.User);
    }

    public async Task Tick(TimeSpan deltaTime) {
        if (!InBattleAsTank) return;

        await Tank!.Tick(deltaTime);
    }

    public override int GetHashCode() => PlayerConnection.GetHashCode();
}
