using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Database.Models;

public class Player {
    public Player(ILogger connectionLogger, string username, string email) : this(username, email) =>
        Logger = connectionLogger.ForType(typeof(Player));

    Player(string username, string email) {
        List<string> admins = ["C6OI"];

        Logger = Log.Logger.ForType(typeof(Player));
        
        CurrentAvatarId = GlobalEntities.GetEntity("avatars", "Tankist").Id;
        Username = username;
        Email = email;

        long defaultWeaponId = GlobalEntities.GetEntity("weapons", "Smoky").Id;
        long defaultHullId = GlobalEntities.GetEntity("hulls", "Hunter").Id;

        long defaultShellId = GlobalEntities.DefaultShells[defaultWeaponId];
        long defaultWeaponSkinId = GlobalEntities.DefaultSkins[defaultWeaponId];

        long defaultHullSkinId = GlobalEntities.DefaultSkins[defaultHullId];
        
        Hulls = [new Hull(this, defaultHullId, defaultHullSkinId)];
        HullSkins = [new HullSkin(this, defaultHullSkinId, defaultHullId)];
        Paints = [new Paint(this, GlobalEntities.GetEntity("paints", "Green").Id)];

        Weapons = [new Weapon(this, defaultWeaponId, defaultWeaponSkinId, defaultShellId)];
        WeaponSkins = [new WeaponSkin(this, defaultWeaponSkinId, defaultWeaponId)];
        Covers = [new Cover(this, GlobalEntities.GetEntity("covers", "None").Id)];
        Shells = [new Shell(this, defaultShellId, defaultWeaponId)];
        
        Avatars = [new Avatar(this, CurrentAvatarId)];
        Graffities = [new Graffiti(this, GlobalEntities.GetEntity("graffities", "Logo").Id)];
        Presets = [new Preset(this, 0)];

        if (admins.Contains(Username))
            Groups |= PlayerGroups.Admin;
    }

    public ILogger Logger { get; }

    public uint Id { get; private init; }

    public string Username { get; set; }
    public string Email { get; set; }

    public bool RememberMe { get; set; }

    public byte[] AutoLoginToken { get; set; } = [];
    public byte[] PasswordHash { get; set; } = [];
    public string HardwareFingerprint { get; set; } = "";

    public PlayerGroups Groups { get; set; }
    public bool IsAdmin => (Groups & PlayerGroups.Admin) == PlayerGroups.Admin;
    public bool IsModerator => IsAdmin || (Groups & PlayerGroups.Moderator) == PlayerGroups.Moderator;
    public bool IsTester => (Groups & PlayerGroups.Tester) == PlayerGroups.Tester;
    public bool IsPremium => (Groups & PlayerGroups.Premium) == PlayerGroups.Premium;
    public bool IsBanned => (Groups & PlayerGroups.Banned) == PlayerGroups.Banned;

    public bool Subscribed { get; set; }
    public string CountryCode { get; set; } = "RU";

    public long CurrentAvatarId { get; set; }
    public int CurrentPresetIndex { get; set; }

    public long Crystals { get; set; }
    public long XCrystals { get; set; }

    public int GoldBoxItems { get; set; }

    public long Experience { get; set; }
    public int Rank => Leveling.GetRank(Experience);

    public List<Avatar> Avatars { get; private set; }
    public List<Cover> Covers { get; private set; }
    public List<Paint> Paints { get; private set; }
    public List<Hull> Hulls { get; private set; }
    public List<HullSkin> HullSkins { get; private set; }
    public List<Graffiti> Graffities { get; private set; }
    public List<Shell> Shells { get; private set; }
    public List<Weapon> Weapons { get; private set; }
    public List<WeaponSkin> WeaponSkins { get; private set; }

    public List<Preset> Presets { get; private set; }

    public DateTimeOffset RegistrationTime { get; init; }
    public DateTimeOffset LastLoginTime { get; set; }

    public List<long> AcceptedFriendIds { get; set; } = [];
    public List<long> IncomingFriendIds { get; set; } = [];
    public List<long> OutgoingFriendIds { get; set; } = [];

    public List<long> BlockedPlayerIds { get; set; } = [];
    public List<long> ReportedPlayerIds { get; set; } = [];
}

[Flags]
public enum PlayerGroups {
    None = 0,
    Admin = 1,
    Moderator = 2,
    Tester = 4,
    Premium = 8,
    Banned = 16
}
