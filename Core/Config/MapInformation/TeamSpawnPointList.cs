using Vint.Core.ECS.Enums;

namespace Vint.Core.Config.MapInformation;

public readonly record struct TeamSpawnPointList(
    List<SpawnPoint> RedTeam,
    List<SpawnPoint> BlueTeam
) {
    public List<SpawnPoint> GetFor(TeamColor teamColor) => teamColor switch {
        TeamColor.Red => RedTeam,
        TeamColor.Blue => BlueTeam,
        _ => throw new ArgumentOutOfRangeException(nameof(teamColor), teamColor, null)
    };
}
