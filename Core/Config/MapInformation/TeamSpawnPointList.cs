namespace Vint.Core.Config.MapInformation;

public class TeamSpawnPointList {
    public IList<SpawnPoint> RedTeam { get; set; } = null!;
    public IList<SpawnPoint> BlueTeam { get; set; } = null!;
}