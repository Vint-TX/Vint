using Vint.Core.Battle.Mode;

namespace Vint.Core.Config.MapInformation;

public record struct MapInfo(
    string Name,
    string ModelPath,
    long Id,
    int MaxPlayers,
    bool Matchmaking,
    MapFlags Flags,
    List<PuntativeGeometry> PuntativeGeoms,
    MapSpawnPointInfo SpawnPoints,
    List<TeleportPoint> TeleportPoints,
    MapBonusInfo BonusRegions
) {
    bool HasSpawnPoints(BattleMode mode) => mode switch {
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
}
