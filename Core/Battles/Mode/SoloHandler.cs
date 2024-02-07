using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public abstract class SoloHandler(
    Battle battle
) : ModeHandler(battle) {
    protected abstract List<SpawnPoint> SpawnPoints { get; }
    protected SpawnPoint? LastSpawnPoint { get; set; }

    /*public IEnumerable<UserResult> UserResults => Battle.Players
        .ToList()
        .Where(battlePlayer => battlePlayer.InBattleAsTank)
        .Select(battlePlayer => battlePlayer.Tank!.UserResult);*/

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

    public override void OnWarmUpCompleted() { }

    public override void OnFinished() { }

    public override void TransferParameters(ModeHandler previousHandler) {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => !battlePlayer.IsSpectator))
            battlePlayer.Team = null;
    }

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        BattlePlayer tankPlayer = new(player, Battle, null, false);
        return tankPlayer;
    }

    public override void PlayerEntered(BattlePlayer player) { }

    public override void PlayerExited(BattlePlayer player) { }
}