using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Serilog;
using Vint.Core.Database.Converters;
using Vint.Core.Database.Models;
using Vint.Core.ECS;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Database;

public sealed class DatabaseContext : DbContext {
    public DatabaseContext() => Database.EnsureCreated();

    ILogger Logger { get; } = Log.Logger.ForType(typeof(DatabaseContext));

    public DbSet<Player> Players { get; private set; } = null!;
    public DbSet<Preset> Presets { get; private set; } = null!;
    public DbSet<Avatar> Avatars { get; private set; } = null!;
    public DbSet<Cover> Covers { get; private set; } = null!;
    public DbSet<Graffiti> Graffities { get; private set; } = null!;
    public DbSet<Hull> Hulls { get; private set; } = null!;
    public DbSet<HullSkin> HullSkins { get; private set; } = null!;
    public DbSet<Paint> Paints { get; private set; } = null!;
    public DbSet<Shell> Shells { get; private set; } = null!;
    public DbSet<Weapon> Weapons { get; private set; } = null!;
    public DbSet<WeaponSkin> WeaponSkins { get; private set; } = null!;
    
    public void Save() {
        int savedChanges = SaveChanges();

        if (savedChanges != 0)
            Logger.Debug("Saved changes to the database: {Count}", savedChanges);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<IEntity>().HaveConversion<EntityToIdConverter>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        DatabaseConfig config = DatabaseConfig.Get();

        MySqlConnectionStringBuilder stringBuilder = new() {
            Server = config.Host,
            Port = config.Port,
            UserID = config.Username,
            Password = config.Password,
            Database = config.Database
        };

        optionsBuilder.UseMySql(new MySqlConnection(stringBuilder.ToString()),
            new MariaDbServerVersion(Version.Parse(config.Version)));
    }
}
