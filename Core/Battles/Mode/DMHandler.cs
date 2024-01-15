using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Enums;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public class DMHandler(
    Battle battle
) : ModeHandler(battle) {
    List<SpawnPoint> SpawnPoints { get; set; } = battle.MapInfo.SpawnPoints.Deathmatch.ToList();
    SpawnPoint LastSpawnPoint { get; set; } = null!;

    public override BattleMode BattleMode => BattleMode.DM;

    public override void Tick() { }

    public override SpawnPoint GetRandomSpawnPoint() =>
        LastSpawnPoint = SpawnPoints
            .Shuffle()
            .First(spawnPoint => spawnPoint != LastSpawnPoint);

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

    public override void RemoveBattlePlayer(BattlePlayer player) =>
        player.PlayerConnection.User.RemoveComponent<TeamColorComponent>();

    public override void PlayerEntered(BattlePlayer player) { }

    public override void PlayerExited(BattlePlayer player) { }
}