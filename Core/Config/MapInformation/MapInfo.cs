using Vint.Core.Battles;

namespace Vint.Core.Config.MapInformation;

// Copied from https://github.com/Assasans/TXServer-Public/blob/database/TXServer/Library/ServerMapInfo.json
public class MapInfo {
    public string Name { get; set; } = null!;
    public long MapId { get; set; }
    public int MaxPlayers { get; set; }
    public bool MatchMaking { get; set; }
    public bool Custom { get; set; }
    public float GoldProbability { get; set; }
    public MapFlags Flags { get; set; } = null!;
    public IList<PuntativeGeometry> PuntativeGeoms { get; set; } = null!;
    public MapSpawnPointInfo SpawnPoints { get; set; } = null!;
    public IList<TeleportPoint> TeleportPoints { get; set; } = null!;
    public MapBonusInfo BonusRegions { get; set; } = null!;

    public bool HasSpawnPoints(BattleMode mode) => mode switch {
        BattleMode.DM => SpawnPoints.Deathmatch != null!,
        BattleMode.TDM => SpawnPoints.TeamDeathmatch != null!,
        BattleMode.CTF => SpawnPoints.CaptureTheFlag != null!,
        _ => false
    };

    public void InitializeDefaultSpawnPoints(BattleMode mode) {
        switch (mode) {
            case BattleMode.DM:
                SpawnPoints.Deathmatch = [new SpawnPoint()];
                break;

            case BattleMode.TDM:
                SpawnPoints.TeamDeathmatch = new TeamSpawnPointList { BlueTeam = [new SpawnPoint()], RedTeam = [new SpawnPoint()] };
                break;

            case BattleMode.CTF:
                SpawnPoints.CaptureTheFlag = new TeamSpawnPointList { BlueTeam = [new SpawnPoint()], RedTeam = [new SpawnPoint()] };
                break;
        }
    }
}