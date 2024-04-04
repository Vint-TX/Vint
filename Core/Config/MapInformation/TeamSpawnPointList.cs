namespace Vint.Core.Config.MapInformation;

public readonly record struct TeamSpawnPointList(
    List<SpawnPoint> RedTeam,
    List<SpawnPoint> BlueTeam
);