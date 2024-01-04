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
}