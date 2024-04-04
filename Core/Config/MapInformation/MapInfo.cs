using Vint.Core.Battles;

namespace Vint.Core.Config.MapInformation;

// Copied from https://github.com/Assasans/TXServer-Public/blob/database/TXServer/Library/ServerMapInfo.json
public record struct MapInfo(
    string Name,
    long Id,
    int MaxPlayers,
    bool MatchMaking,
    bool Custom,
    float GoldProbability,
    MapFlags Flags,
    List<PuntativeGeometry> PuntativeGeoms,
    MapSpawnPointInfo SpawnPoints,
    List<TeleportPoint> TeleportPoints,
    MapBonusInfo BonusRegions
) {
    public bool HasSpawnPoints(BattleMode mode) => mode switch {
        BattleMode.DM => SpawnPoints.Deathmatch != null!,
        BattleMode.TDM => SpawnPoints.TeamDeathmatch != null,
        BattleMode.CTF => SpawnPoints.CaptureTheFlag != null,
        _ => false
    };

    public void InitializeDefaultSpawnPoints(BattleMode mode) {
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