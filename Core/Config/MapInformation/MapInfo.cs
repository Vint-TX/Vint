using System.Numerics;
using BepuPhysics.Collidables;
using SharpGLTF.Schema2;
using Vint.Core.ECS.Enums;

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
    static Vector3 GltfToUnity { get; } = new(-1, 1, 1);

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
        string mapModelPath = Path.Combine(ConfigPath, "model.glb");
        ModelRoot mapRoot = ModelRoot.Load(mapModelPath);

        Triangle[] triangles = mapRoot
            .DefaultScene // todo create a mesh immediately instead of store list of triangles
            .EvaluateTriangles()
            .Select(tuple => new Triangle(
                tuple.A.GetGeometry().GetPosition() * GltfToUnity,
                tuple.B.GetGeometry().GetPosition() * GltfToUnity,
                tuple.C.GetGeometry().GetPosition() * GltfToUnity))
            .ToArray();

        return triangles;
    }
}
