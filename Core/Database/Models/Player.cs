using LinqToDB;
using LinqToDB.Mapping;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Database.Models;

[Table("Players")]
public class Player {
    [NotColumn] public ILogger Logger { get; } = Log.Logger.ForType(typeof(Player));

    [PrimaryKey] public long Id { get; init; }

    [Column(DataType = DataType.Text)] public string Username { get; set; } = null!;
    [Column(DataType = DataType.Text)] public string Email { get; set; } = null!;

    [Column] public bool RememberMe { get; set; }

    [Column] public byte[] AutoLoginToken { get; set; } = [];
    [Column] public byte[] PasswordHash { get; set; } = [];
    [Column(DataType = DataType.Text)] public string HardwareFingerprint { get; set; } = "";

    [Column] public PlayerGroups Groups { get; set; }
    [NotColumn] public bool IsAdmin => (Groups & PlayerGroups.Admin) == PlayerGroups.Admin;
    [NotColumn] public bool IsModerator => IsAdmin || (Groups & PlayerGroups.Moderator) == PlayerGroups.Moderator;
    [NotColumn] public bool IsTester => (Groups & PlayerGroups.Tester) == PlayerGroups.Tester;
    [NotColumn] public bool IsPremium => (Groups & PlayerGroups.Premium) == PlayerGroups.Premium;
    [NotColumn] public bool IsBanned => (Groups & PlayerGroups.Banned) == PlayerGroups.Banned;

    [Column] public RewardedLeagues RewardedLeagues { get; set; }

    [Column] public bool Subscribed { get; set; }
    [Column(DataType = DataType.Text)] public string CountryCode { get; set; } = "RU";

    [Column] public long CurrentAvatarId { get; set; }
    [Column] public int CurrentPresetIndex { get; set; }

    [NotColumn] public List<Preset> UserPresets { get; set; } = [];
    [NotColumn] public Preset CurrentPreset => UserPresets.Single(preset => preset.Index == CurrentPresetIndex);

    [Column] public long Crystals { get; set; } = 1000000;
    [Column] public long XCrystals { get; set; } = 1000000;

    [Column] public int GoldBoxItems { get; set; }

    [Column] public long Experience { get; set; }
    [NotColumn] public int Rank => Leveling.GetRank(Experience);

    [Column(DataType = DataType.Text)] public string FractionName { get; set; } = "";
    [Column] public long FractionScore { get; set; }

    [NotColumn] public IEntity Fraction => GlobalEntities.GetEntity("fractions", FractionName);

    [Column] public DateTimeOffset RegistrationTime { get; init; }
    [Column] public DateTimeOffset LastLoginTime { get; set; }

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Statistics.PlayerId))]
    public Statistics Stats { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(ReputationStatistics.PlayerId))]
    public List<ReputationStatistics> ReputationStats { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(SeasonStatistics.PlayerId))]
    public List<SeasonStatistics> SeasonStats { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Avatar.PlayerId))]
    public List<Avatar> Avatars { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Cover.PlayerId))]
    public List<Cover> Covers { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Graffiti.PlayerId))]
    public List<Graffiti> Graffities { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Hull.PlayerId))]
    public List<Hull> Hulls { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(HullSkin.PlayerId))]
    public List<HullSkin> HullSkins { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Module.PlayerId))]
    public List<Module> Modules { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Paint.PlayerId))]
    public List<Paint> Paints { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Preset.PlayerId))]
    public List<Preset> Presets { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Relation.SourcePlayerId))]
    public List<Relation> Relations { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Shell.PlayerId))]
    public List<Shell> Shells { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Weapon.PlayerId))]
    public List<Weapon> Weapons { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(WeaponSkin.PlayerId))]
    public List<WeaponSkin> WeaponSkins { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Container.PlayerId))]
    public List<Container> Containers { get; private set; } = null!;

    public void InitializeNew() {
        CurrentAvatarId = GlobalEntities.GetEntity("avatars", "Tankist").Id;

        long weaponId = GlobalEntities.GetEntity("weapons", "Smoky").Id;
        long hullId = GlobalEntities.GetEntity("hulls", "Hunter").Id;

        long shellId = GlobalEntities.DefaultShells[weaponId];
        long weaponSkinId = GlobalEntities.DefaultSkins[weaponId];
        long coverId = GlobalEntities.GetEntity("covers", "None").Id;

        long hullSkinId = GlobalEntities.DefaultSkins[hullId];
        long paintId = GlobalEntities.GetEntity("paints", "Green").Id;

        long graffitiId = GlobalEntities.GetEntity("graffities", "Logo").Id;

        using (DbConnection db = new()) {
            db.Insert(new Hull { Player = this, SkinId = hullSkinId, Id = hullId });
            db.Insert(new HullSkin { Player = this, HullId = hullId, Id = hullSkinId });
            db.Insert(new Paint { Player = this, Id = paintId });

            db.Insert(new Weapon { Player = this, Id = weaponId, SkinId = weaponSkinId, ShellId = shellId });
            db.Insert(new WeaponSkin { Player = this, WeaponId = weaponId, Id = weaponSkinId });
            db.Insert(new Cover { Player = this, Id = coverId });
            db.Insert(new Shell { Player = this, Id = shellId, WeaponId = weaponId });

            db.Insert(new Avatar { Player = this, Id = CurrentAvatarId });
            db.Insert(new Graffiti { Player = this, Id = graffitiId });

            db.Insert(new Preset { Player = this, Index = 0, Name = "Preset 1" });

            db.Insert(new SeasonStatistics { Player = this, Reputation = 100, SeasonNumber = 1 /*todo SeasonNumber*/ });
            db.Insert(new Statistics { Player = this });
        }

        List<string> admins = ["C6OI"];
        List<string> testers = ["C6OI"];

        if (admins.Contains(Username))
            Groups |= PlayerGroups.Admin;

        if (testers.Contains(Username))
            Groups |= PlayerGroups.Tester;
    }
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

[Flags]
public enum RewardedLeagues {
    None = 0,
    Training = 1,
    Bronze = 2,
    Silver = 4,
    Gold = 8,
    Master = 16
}