using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battles;

public class BattleProperties() {
    public BattleProperties(BattleMode battleMode,
        GravityType gravity,
        long mapId,
        bool friendlyFire,
        bool killZoneEnabled,
        bool damageEnabled,
        bool disabledModules,
        int maxPlayers,
        int timeLimit,
        int scoreLimit) : this() {
        BattleMode = battleMode;
        Gravity = gravity;
        MapId = mapId;
        FriendlyFire = friendlyFire;
        KillZoneEnabled = killZoneEnabled;
        DamageEnabled = damageEnabled;
        DisabledModules = disabledModules;
        MaxPlayers = maxPlayers;
        TimeLimit = timeLimit;
        ScoreLimit = scoreLimit;
    }

    public BattleMode BattleMode { get; private set; }
    public GravityType Gravity { get; private set; }
    public long MapId { get; private set; }
    public bool FriendlyFire { get; private set; }
    public bool KillZoneEnabled { get; private set; }
    [ProtocolIgnore] public bool DamageEnabled { get; set; }
    public bool DisabledModules { get; private set; }
    public int MaxPlayers { get; private set; }
    public int TimeLimit { get; private set; }
    public int ScoreLimit { get; private set; }

    [ProtocolIgnore] public static IReadOnlyDictionary<GravityType, float> GravityToForce { get; set; } = new Dictionary<GravityType, float> {
        { GravityType.Earth, 9.81f },
        { GravityType.Moon, 1.62f },
        { GravityType.Mars, 3.71f },
        { GravityType.SuperEarth, 30f }
    };
}

public enum BattleMode : byte {
    DM = 0,
    TDM = 1,
    CTF = 2
}

public enum GravityType : byte {
    Earth = 0,
    Moon = 1,
    Mars = 2,
    SuperEarth = 3
}
