namespace Vint.Core.Config.MapInformation;

public class MapSpawnPointInfo {
    public IList<SpawnPoint> Deathmatch { get; set; } = null!;
    public TeamSpawnPointList TeamDeathatch { get; set; } = null!;
    public TeamSpawnPointList CaptureTheFlag { get; set; } = null!;
}