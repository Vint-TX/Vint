using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Enums;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public class DMHandler(
    Battle battle
) : SoloHandler(battle) {
    List<SpawnPoint> SpawnPoints { get; set; } = battle.MapInfo.SpawnPoints.Deathmatch.ToList();
    SpawnPoint? LastSpawnPoint { get; set; }

    public override void Tick() { }

    public override SpawnPoint GetRandomSpawnPoint(BattlePlayer battlePlayer) {
        SpawnPoint spawnPoint = SpawnPoints
            .Shuffle()
            .First(spawnPoint => spawnPoint.Number != LastSpawnPoint?.Number &&
                                 spawnPoint.Number != battlePlayer.Tank?.SpawnPoint?.Number &&
                                 spawnPoint.Number != battlePlayer.Tank?.PreviousSpawnPoint?.Number);

        LastSpawnPoint = spawnPoint;
        return spawnPoint;
    }

    public override void SortPlayers() => SortPlayers(Battle.Players.ToList());

    public override void OnStarted() { }

    public override void OnFinished() { }

    public override void TransferParameters(ModeHandler previousHandler) {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => !battlePlayer.IsSpectator)) {
            battlePlayer.Team = null;
            battlePlayer.PlayerConnection.User.ChangeComponent<TeamColorComponent>(component =>
                component.TeamColor = TeamColor.None);
        }
    }

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        BattlePlayer tankPlayer = new(player, Battle, null, false);

        player.User.AddComponent(new TeamColorComponent(TeamColor.None));
        return tankPlayer;
    }

    public override void PlayerEntered(BattlePlayer player) { }

    public override void PlayerExited(BattlePlayer player) { }
}