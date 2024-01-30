using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public abstract class TeamHandler(
    Battle battle
) : ModeHandler(battle) {
    public IEntity RedTeam { get; } = new TeamTemplate().Create(TeamColor.Red, battle.BattleEntity);
    public IEntity BlueTeam { get; } = new TeamTemplate().Create(TeamColor.Blue, battle.BattleEntity);
    protected IEntity TeamChat { get; } = new TeamBattleChatTemplate().Create();

    public IEnumerable<BattlePlayer> RedPlayers => Battle.Players
        .Where(battlePlayer => battlePlayer.Team?.GetComponent<TeamColorComponent>().TeamColor == TeamColor.Red);

    public IEnumerable<BattlePlayer> BluePlayers => Battle.Players
        .Where(battlePlayer => battlePlayer.Team?.GetComponent<TeamColorComponent>().TeamColor == TeamColor.Blue);

    protected SpawnPoint? LastRedSpawnPoint { get; set; }
    protected SpawnPoint? LastBlueSpawnPoint { get; set; }
    protected abstract List<SpawnPoint> RedSpawnPoints { get; }
    protected abstract List<SpawnPoint> BlueSpawnPoints { get; }

    public override void SortPlayers() {
        SortPlayers(RedPlayers.ToList());
        SortPlayers(BluePlayers.ToList());
    }

    public override SpawnPoint GetRandomSpawnPoint(BattlePlayer battlePlayer) {
        TeamColor teamColor;
        List<SpawnPoint> spawnPoints;

        if (battlePlayer.Team == RedTeam) {
            spawnPoints = RedSpawnPoints;
            teamColor = TeamColor.Red;
        } else if (battlePlayer.Team == BlueTeam) {
            spawnPoints = BlueSpawnPoints;
            teamColor = TeamColor.Blue;
        } else throw new ArgumentException("Unexpected team");

        SpawnPoint spawnPoint = spawnPoints
            .Shuffle()
            .First(spawnPoint => spawnPoint.Number != LastRedSpawnPoint?.Number &&
                                 spawnPoint.Number != LastBlueSpawnPoint?.Number &&
                                 spawnPoint.Number != battlePlayer.Tank?.SpawnPoint?.Number &&
                                 spawnPoint.Number != battlePlayer.Tank?.PreviousSpawnPoint?.Number);

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
                battlePlayer.PlayerConnection.User.ChangeComponent(new TeamColorComponent(currentColor));

                players.Remove(battlePlayer);
                currentColor = currentColor == TeamColor.Red ? TeamColor.Blue : TeamColor.Red;
            }
        }
    }

    public override void PlayerEntered(BattlePlayer player) =>
        player.PlayerConnection.Share(TeamChat, RedTeam, BlueTeam);

    public override void PlayerExited(BattlePlayer player) =>
        player.PlayerConnection.Unshare(TeamChat, RedTeam, BlueTeam);

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        IEntity team = RedPlayers.Count() < BluePlayers.Count() ? RedTeam : BlueTeam;
        BattlePlayer battlePlayer = new(player, Battle, team, false);

        player.User.AddComponent(team.GetComponent<TeamColorComponent>());
        return battlePlayer;
    }
}