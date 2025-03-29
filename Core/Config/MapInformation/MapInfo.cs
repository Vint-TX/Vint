using OpenTK.Mathematics;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Schema2;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Simulations.Geometry;

namespace Vint.Core.Config.MapInformation;

public record struct MapInfo(
    string Name,
    long Id,
    int MaxPlayers,
    bool Matchmaking,
    MapFlags Flags,
    List<PuntativeGeometry> PuntativeGeoms,
    MapSpawnPointInfo SpawnPoints,
    List<TeleportPoint> TeleportPoints,
    MapBonusInfo BonusRegions
) {
    string ConfigPath { get; set; } = null!;
    public Lazy<Triangle[]> Triangles { get; private set; }

    public void Init() {
        ConfigPath = Path.Combine(ConfigManager.ResourcesPath, "Maps", Name);
        Triangles = new Lazy<Triangle[]>(GetTriangles, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public bool HasSpawnPoints(BattleMode mode) => mode switch {
        BattleMode.DM => SpawnPoints.Deathmatch != null!,
        BattleMode.TDM => SpawnPoints.TeamDeathmatch != null,
        BattleMode.CTF => SpawnPoints.CaptureTheFlag != null,
        _ => false
    };

    public void InitDefaultSpawnPointsIfAbsent(BattleMode mode) {
        if (HasSpawnPoints(mode)) return;

        switch (mode) {
            case BattleMode.DM:
                SpawnPoints = SpawnPoints with { Deathmatch = [new SpawnPoint()] };
                break;

            case BattleMode.TDM:
                SpawnPoints = SpawnPoints with {
                    TeamDeathmatch = new TeamSpawnPointList {
                        BlueTeam = [new SpawnPoint()],
                        RedTeam = [new SpawnPoint()]
                    }
                };

                break;

            case BattleMode.CTF:
                SpawnPoints = SpawnPoints with {
                    CaptureTheFlag = new TeamSpawnPointList {
                        BlueTeam = [new SpawnPoint()],
                        RedTeam = [new SpawnPoint()]
                    }
                };

                break;
        }
    }

    Triangle[] GetTriangles() {
        ModelRoot meshRoot = ModelRoot.Load(Path.Combine(ConfigPath, "model.glb"));

        Triangle[] triangles = meshRoot.DefaultScene
            .EvaluateTriangles()
            .Select(tuple => new Triangle(
                CreateVertex(tuple.A.GetGeometry()),
                CreateVertex(tuple.B.GetGeometry()),
                CreateVertex(tuple.C.GetGeometry())))
            .ToArray();

        return triangles;
    }

    static Vertex CreateVertex(IVertexGeometry vertexBuilder) {
        Vector3 position = (Vector3)vertexBuilder.GetPosition();
        vertexBuilder.TryGetNormal(out System.Numerics.Vector3 normal);

        return new Vertex(position, (Vector3)normal);
    }
}
