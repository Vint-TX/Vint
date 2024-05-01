using LinqToDB;
using LinqToDB.Mapping;
using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Database.Models;

[Table("Players")]
public class Player {
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

    [Column] public League RewardedLeagues { get; set; }

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

    [Column] public uint Reputation { get; set; }
    [Column] public long GameplayChestScore { get; set; }
    [Column] public long DesertedBattlesCount { get; set; }
    [Column] public int NeedGoodBattlesCount { get; set; }

    [NotColumn] public League League => Reputation switch {
        < 139 => League.Training,
        < 999 => League.Bronze,
        < 2999 => League.Silver,
        < 4499 => League.Gold,
        < 99999 => League.Master,
        _ => League.None
    };

    [NotColumn] public int MinReputationDelta => League switch {
        League.Training => -3,
        League.Bronze => -20,
        League.Silver => -30,
        League.Gold => -35,
        League.Master => -40,
        _ => 0
    };

    [NotColumn] public int MaxReputationDelta => League switch {
        League.Training => 20,
        League.Bronze => 40,
        League.Silver => 35,
        League.Gold => 30,
        League.Master => 20,
        _ => 0
    };

    [NotColumn] public IEntity LeagueEntity => GlobalEntities.GetEntity("leagues", League.ToString());

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

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Punishment.PlayerId))]
    public List<Punishment> Punishments { get; private set; } = null!;

    public async Task InitializeNew() {
        CurrentAvatarId = GlobalEntities.GetEntity("avatars", "Tankist").Id;
        Reputation = 100;

        long weaponId = GlobalEntities.GetEntity("weapons", "Smoky").Id;
        long hullId = GlobalEntities.GetEntity("hulls", "Hunter").Id;

        long shellId = GlobalEntities.DefaultShells[weaponId];
        long weaponSkinId = GlobalEntities.DefaultSkins[weaponId];
        long coverId = GlobalEntities.GetEntity("covers", "None").Id;

        long hullSkinId = GlobalEntities.DefaultSkins[hullId];
        long paintId = GlobalEntities.GetEntity("paints", "Green").Id;

        long graffitiId = GlobalEntities.GetEntity("graffities", "Logo").Id;

        await using (DbConnection db = new()) {
            await db.BeginTransactionAsync();

            await db.InsertAsync(new Hull { Player = this, SkinId = hullSkinId, Id = hullId });
            await db.InsertAsync(new HullSkin { Player = this, HullId = hullId, Id = hullSkinId });
            await db.InsertAsync(new Paint { Player = this, Id = paintId });

            await db.InsertAsync(new Weapon { Player = this, Id = weaponId, SkinId = weaponSkinId, ShellId = shellId });
            await db.InsertAsync(new WeaponSkin { Player = this, WeaponId = weaponId, Id = weaponSkinId });
            await db.InsertAsync(new Cover { Player = this, Id = coverId });
            await db.InsertAsync(new Shell { Player = this, Id = shellId, WeaponId = weaponId });

            await db.InsertAsync(new Avatar { Player = this, Id = CurrentAvatarId });
            await db.InsertAsync(new Graffiti { Player = this, Id = graffitiId });

            await db.InsertAsync(new Preset { Player = this, Index = 0, Name = "Preset 1" });

            await db.InsertAsync(new SeasonStatistics { Player = this, Reputation = 100, SeasonNumber = ConfigManager.SeasonNumber });
            await db.InsertAsync(new Statistics { Player = this });

            await db.CommitTransactionAsync();
        }

        List<string> admins = ["C6OI"];
        List<string> testers = ["C6OI"];

        if (admins.Contains(Username)) {
            Groups |= PlayerGroups.Admin;
            Groups |= PlayerGroups.Moderator;
        }

        if (testers.Contains(Username))
            Groups |= PlayerGroups.Tester;

        Modules = [];
    }

    public async Task<Punishment> Warn(string? reason, TimeSpan? duration) {
        Punishment punishment = new() {
            Player = this,
            PunishTime = DateTimeOffset.UtcNow,
            Active = true,
            Duration = duration?.Duration(),
            Reason = reason,
            Type = PunishmentType.Warn
        };

        await using DbConnection db = new();

        punishment.Id = await db.InsertWithInt64IdentityAsync(punishment);
        return punishment;
    }

    public async Task<Punishment> Mute(string? reason, TimeSpan? duration) {
        await UnMute();

        Punishment punishment = new() {
            Player = this,
            PunishTime = DateTimeOffset.UtcNow,
            Active = true,
            Duration = duration?.Duration(),
            Reason = reason,
            Type = PunishmentType.Mute
        };

        await using DbConnection db = new();

        punishment.Id = await db.InsertWithInt64IdentityAsync(punishment);
        return punishment;
    }

    public async Task<Punishment> Ban(string? reason, TimeSpan? duration) {
        await UnBan();

        Punishment punishment = new() {
            Player = this,
            PunishTime = DateTimeOffset.UtcNow,
            Active = true,
            Duration = duration?.Duration(),
            Reason = reason,
            Type = PunishmentType.Ban
        };

        await using DbConnection db = new();

        punishment.Id = await db.InsertWithInt64IdentityAsync(punishment);
        return punishment;
    }

    public async Task<bool> UnWarn(long warnId) {
        await using DbConnection db = new();

        Punishment? punishment = await db.Punishments
            .Where(punishment => punishment.PlayerId == Id &&
                                 punishment.Type == PunishmentType.Warn)
            .SingleOrDefaultAsync(punishment => punishment.Id == warnId);

        if (punishment == null) return false;

        punishment.Active = false;
        await db.UpdateAsync(punishment);
        return true;
    }

    public async Task<bool> UnMute() {
        await using DbConnection db = new();

        Punishment? punishment = await db.Punishments
            .Where(punishment => punishment.PlayerId == Id &&
                                 punishment.Type == PunishmentType.Mute &&
                                 punishment.Active)
            .OrderByDescending(punishment => punishment.PunishTime)
            .FirstOrDefaultAsync();

        if (punishment == null) return false;

        punishment.Active = false;
        await db.UpdateAsync(punishment);
        return true;
    }

    public async Task<bool> UnBan() {
        await using DbConnection db = new();

        Punishment? punishment = await db.Punishments
            .Where(punishment => punishment.PlayerId == Id &&
                                 punishment.Type == PunishmentType.Ban &&
                                 punishment.Active)
            .OrderByDescending(punishment => punishment.PunishTime)
            .FirstOrDefaultAsync();

        if (punishment == null) return false;

        punishment.Active = false;
        await db.UpdateAsync(punishment);
        return true;
    }

    public async Task<Punishment?> GetBanInfo() {
        await RefreshPunishments();

        await using DbConnection db = new();
        return await db.Punishments
            .Where(punishment => punishment.PlayerId == Id &&
                                 punishment.Type == PunishmentType.Ban &&
                                 punishment.Active)
            .OrderByDescending(punishment => punishment.PunishTime)
            .FirstOrDefaultAsync();
    }

    public async Task<Punishment?> GetMuteInfo() {
        await RefreshPunishments();

        await using DbConnection db = new();
        return await db.Punishments
            .Where(punishment => punishment.PlayerId == Id &&
                                 punishment.Type == PunishmentType.Mute &&
                                 punishment.Active)
            .OrderByDescending(punishment => punishment.PunishTime)
            .FirstOrDefaultAsync();
    }

    async Task RefreshPunishments() {
        await using DbConnection db = new();

        try {
            await db.BeginTransactionAsync();

            foreach (Punishment punishment in db.Punishments
                         .Where(punishment => punishment.PlayerId == Id &&
                                              punishment.Active)
                         .ToList()
                         .Where(punishment => punishment.EndTime <= DateTimeOffset.UtcNow)) {
                punishment.Active = false;
                await db.UpdateAsync(punishment);
            }

            await db.CommitTransactionAsync();
        } catch {
            await db.RollbackTransactionAsync();
            throw;
        } finally {
            await db.DisposeTransactionAsync();
        }
    }
}

[Flags]
public enum PlayerGroups {
    None = 0,
    Admin = 1,
    Moderator = 2,
    Tester = 4,
    Premium = 8
}

[Flags]
public enum League {
    None = 0,
    Training = 1,
    Bronze = 2,
    Silver = 4,
    Gold = 8,
    Master = 16
}
