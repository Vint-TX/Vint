using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;

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

    public override int CalculateReputationDelta(BattlePlayer player) => 0; // todo calculate by K/D
}