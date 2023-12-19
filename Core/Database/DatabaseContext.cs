using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Serilog;
using Vint.Core.ECS;
using Vint.Core.Utils;

namespace Vint.Core.Database;

public sealed class DatabaseContext : DbContext {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(DatabaseContext));

    public DbSet<Player> Players { get; private set; } = null!;

    public DatabaseContext() {
        //Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    public void Save() {
        int savedChanges = SaveChanges();

        if (savedChanges != 0)
            Logger.Debug("Saved changes to the database: {Count}", savedChanges);
    }

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
