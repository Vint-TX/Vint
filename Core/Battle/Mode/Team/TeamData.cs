using Vint.Core.Battle.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battle.Mode.Team;

public class TeamData(
    TeamColor color,
    IEntity entity,
    IList<SpawnPoint> spawnPoints,
    GetRandomSpawnPoint getRandomSpawnPoint,
    Func<IEnumerable<Tanker>> getTankers
) {
    public TeamColor Color { get; } = color;
    public IEntity Entity { get; } = entity;
    public IList<SpawnPoint> SpawnPoints { get; } = spawnPoints;

    public IEnumerable<Tanker> Players => getTankers().Where(tanker => tanker.TeamColor == Color);
    public int Score => Entity.GetComponent<TeamScoreComponent>().Score;

    SpawnPoint LastSpawnPoint { get; set; }

    public SpawnPoint GetRandomSpawnPoint(Tanker tanker) =>
        LastSpawnPoint = getRandomSpawnPoint(SpawnPoints, LastSpawnPoint, tanker.Tank.SpawnPoint, tanker.Tank.PreviousSpawnPoint);
}
