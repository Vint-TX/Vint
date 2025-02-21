using LinqToDB;
using Vint.Core.Battle.Player;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Rewards;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Results;

public class PersonalBattleResultForClient(
    IEntity prevLeague,
    double reputationDelta
) {
    public int CurrentBattleSeries { get; private set; }
    public int MaxBattleSeries => 5;
    public int RankExp { get; private set; }
    public int RankExpDelta { get; private set; }
    public int WeaponExp { get; private set; }
    public int TankLevel { get; private set; }
    public int WeaponLevel { get; private set; }
    public int WeaponInitExp { get; private set; }
    public int WeaponFinalExp { get; private set; }
    public int TankExp { get; private set; }
    public int TankInitExp { get; private set; }
    public int TankFinalExp { get; private set; }
    public int ItemsExpDelta { get; private set; }
    public int ContainerScore { get; private set; }
    public int ContainerScoreDelta { get; private set; }
    public int ContainerScoreLimit => 1000;
    public int LeaguePlace { get; private set; }
    public double Reputation { get; private set; }
    public double ReputationDelta { get; } = reputationDelta;
    public float ScoreBattleSeriesMultiplier { get; private set; }
    public IEntity Container { get; private set; } = null!;
    public IEntity League { get; private set; } = null!;
    public IEntity PrevLeague { get; } = prevLeague;
    public IEntity? Reward { get; private set; }
    public TeamColor UserTeamColor { get; private set; }
    public TeamBattleResult TeamBattleResult { get; private set; }

    /*Energy is not used in the game anymore*/
    public EnergySource? MaxEnergySource => null;

    public async Task Init(IPlayerConnection connection) {
        Tanker tanker = connection.LobbyPlayer?.Tanker!;

        Database.Models.Player player = connection.Player;
        Preset preset = player.CurrentPreset;
        IEntity userHull = preset.Hull.GetUserEntity(connection);
        IEntity userWeapon = preset.Weapon.GetUserEntity(connection);
        IEntity? reward = await Leveling.GetLevelUpRewards(connection);

        if (reward != null)
            await connection.Share(reward);

        int battleScore = tanker.GetBattleUserScoreWithBonus();
        await using DbConnection db = new();

        TankInitExp = (int)userHull.GetComponent<ExperienceItemComponent>().Experience - battleScore;
        TankFinalExp = userHull.GetComponent<ExperienceToLevelUpItemComponent>().FinalLevelExperience;
        TankExp = await db.Hulls
            .Where(hull => hull.PlayerId == player.Id && hull.Id == preset.Hull.Id)
            .Select(hull => (int)hull.Xp)
            .SingleAsync();

        TankLevel = Leveling.GetLevel(TankExp);

        WeaponInitExp = (int)userWeapon.GetComponent<ExperienceItemComponent>().Experience - battleScore;
        WeaponFinalExp = userWeapon.GetComponent<ExperienceToLevelUpItemComponent>().FinalLevelExperience;
        WeaponExp = await db.Weapons
            .Where(weapon => weapon.PlayerId == player.Id && weapon.Id == preset.Weapon.Id)
            .Select(weapon => (int)weapon.Xp)
            .SingleAsync();

        WeaponLevel = Leveling.GetLevel(WeaponExp);

        RankExp = (int)player.Experience;
        RankExpDelta = battleScore;

        Reputation = player.Reputation;
        League = player.LeagueEntity;
        LeaguePlace = await Leveling.GetSeasonPlace(player.Id);

        Container = League.GetComponent<ChestBattleRewardComponent>().Chest;
        ContainerScoreDelta = battleScore;
        ContainerScore = (int)player.GameplayChestScore;

        ItemsExpDelta = battleScore;
        Reward = reward;

        UserTeamColor = connection.UserContainer.Entity.GetComponent<TeamColorComponent>().TeamColor;
        TeamBattleResult = tanker.TeamResult;

        CurrentBattleSeries = connection.BattleSeries;
        ScoreBattleSeriesMultiplier = tanker.ScoreMultiplier;
    }
}
