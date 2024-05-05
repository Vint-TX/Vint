using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public abstract class TeamHandler(
    Battle battle
) : ModeHandler(battle) {
    public IEntity RedTeam { get; } = new TeamTemplate().Create(TeamColor.Red, battle.Entity);
    public IEntity BlueTeam { get; } = new TeamTemplate().Create(TeamColor.Blue, battle.Entity);
    IEntity TeamChat { get; } = new TeamBattleChatTemplate().Create();

    public IEnumerable<BattlePlayer> RedPlayers => Battle.Players
        .Where(battlePlayer => battlePlayer.TeamColor == TeamColor.Red);

    public IEnumerable<BattlePlayer> BluePlayers => Battle.Players
        .Where(battlePlayer => battlePlayer.TeamColor == TeamColor.Blue);

    SpawnPoint? LastRedSpawnPoint { get; set; }
    SpawnPoint? LastBlueSpawnPoint { get; set; }
    protected abstract List<SpawnPoint> RedSpawnPoints { get; }
    protected abstract List<SpawnPoint> BlueSpawnPoints { get; }

    public override void SortPlayers() {
        SortPlayers(RedPlayers);
        SortPlayers(BluePlayers);
    }

    public override void UpdateScore(IEntity? team, int score) {
        if (team == null) return;

        team.ChangeComponent<TeamScoreComponent>(component => component.Score = Math.Max(0, component.Score + score));
        foreach (IPlayerConnection connection in Battle.Players.Where(player => player.InBattle).Select(player => player.PlayerConnection))
            connection.Send(new RoundScoreUpdatedEvent(), Battle.RoundEntity);
    }

    public override void OnWarmUpCompleted() {
        UpdateScore(RedTeam, int.MinValue);
        UpdateScore(BlueTeam, int.MinValue);
    }

    public override SpawnPoint GetRandomSpawnPoint(BattlePlayer battlePlayer) {
        TeamColor teamColor = battlePlayer.TeamColor;

        List<SpawnPoint> spawnPoints = teamColor switch {
            TeamColor.Red => RedSpawnPoints,
            TeamColor.Blue => BlueSpawnPoints,
            _ => throw new ArgumentException("Unexpected team")
        };

        SpawnPoint spawnPoint = spawnPoints
            .Shuffle()
            .FirstOrDefault(spawnPoint => spawnPoint.Number != LastRedSpawnPoint?.Number &&
                                          spawnPoint.Number != LastBlueSpawnPoint?.Number &&
                                          spawnPoint.Number != battlePlayer.Tank?.SpawnPoint.Number &&
                                          spawnPoint.Number != battlePlayer.Tank?.PreviousSpawnPoint.Number,
                spawnPoints.First());

        switch (teamColor) {
            case TeamColor.Red:
                LastRedSpawnPoint = spawnPoint;
                break;

            case TeamColor.Blue:
                LastBlueSpawnPoint = spawnPoint;
                break;
        }

        return spawnPoint;
    }

    public override void TransferParameters(ModeHandler previousHandler) {
        if (previousHandler is TeamHandler) {
            foreach (BattlePlayer battlePlayer in RedPlayers)
                battlePlayer.Team = RedTeam;

            foreach (BattlePlayer battlePlayer in BluePlayers)
                battlePlayer.Team = BlueTeam;
        } else {
            TeamColor currentColor = TeamColor.Red;
            List<BattlePlayer> players = Battle.Players.ToList();

            while (players.Count > 0) {
                players = players.Shuffle();
                BattlePlayer battlePlayer = players.First();

                battlePlayer.Team = currentColor == TeamColor.Red ? RedTeam : BlueTeam;

                players.Remove(battlePlayer);
                currentColor = currentColor == TeamColor.Red ? TeamColor.Blue : TeamColor.Red;
            }
        }
    }

    public override void PlayerEntered(BattlePlayer player) =>
        player.PlayerConnection.Share(TeamChat, RedTeam, BlueTeam);

    public override Task PlayerExited(BattlePlayer player) {
        player.PlayerConnection.Unshare(TeamChat, RedTeam, BlueTeam);
        return Task.CompletedTask;
    }

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        IEntity team = RedPlayers.Count() < BluePlayers.Count() ? RedTeam : BlueTeam;
        BattlePlayer battlePlayer = new(player, Battle, team, false);

        return battlePlayer;
    }

    public abstract TeamColor GetDominatedTeam();
}
