using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battles.Mode;

public class TDMHandler(
    Battle battle
) : TeamHandler(battle) {
    protected override List<SpawnPoint> RedSpawnPoints { get; } = battle.MapInfo.SpawnPoints.TeamDeathmatch.RedTeam.ToList();
    protected override List<SpawnPoint> BlueSpawnPoints { get; } = battle.MapInfo.SpawnPoints.TeamDeathmatch.BlueTeam.ToList();

    public override void Tick() { }

    public override void OnStarted() { }

    public override void OnWarmUpCompleted() { }

    public override void OnFinished() { }

    public override int CalculateReputationDelta(BattlePlayer player) => player.TeamBattleResult switch { // todo calculate by K/D
        TeamBattleResult.Win => player.PlayerConnection.Player.MaxReputationDelta,
        TeamBattleResult.Draw => 0,
        TeamBattleResult.Defeat => player.PlayerConnection.Player.MinReputationDelta,
        _ => -99999
    };
}