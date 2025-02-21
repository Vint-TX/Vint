using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battle.Properties;

public struct ClientBattleParams() {
    public ClientBattleParams(
        BattleMode battleMode,
        GravityType gravity,
        MapInfo mapInfo,
        bool friendlyFire,
        bool killZoneEnabled,
        bool disabledModules,
        int timeLimit
    ) : this(battleMode, gravity, mapInfo.Id, friendlyFire, killZoneEnabled, disabledModules, mapInfo.MaxPlayers, timeLimit) { }

    public ClientBattleParams(
        BattleMode battleMode,
        GravityType gravity,
        long mapId,
        bool friendlyFire,
        bool killZoneEnabled,
        bool disabledModules,
        int maxPlayers,
        int timeLimit
    ) : this() {
        BattleMode = battleMode;
        Gravity = gravity;
        MapId = mapId;
        FriendlyFire = friendlyFire;
        KillZoneEnabled = killZoneEnabled;
        DisabledModules = disabledModules;
        MaxPlayers = maxPlayers;
        TimeLimit = timeLimit;
    }

    public BattleMode BattleMode { get; private set; }
    public GravityType Gravity { get; private set; }
    public long MapId { get; private set; }
    public bool FriendlyFire { get; private set; }
    public bool KillZoneEnabled { get; private set; }
    public bool DisabledModules { get; private set; }
    public int MaxPlayers { get; private set; }
    public int TimeLimit { get; private set; }
}
