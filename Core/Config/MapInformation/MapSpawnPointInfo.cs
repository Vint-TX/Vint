namespace Vint.Core.Config.MapInformation;

public readonly record struct MapSpawnPointInfo(
    List<SpawnPoint> Deathmatch,
    TeamSpawnPointList? TeamDeathmatch,
    TeamSpawnPointList? CaptureTheFlag
);