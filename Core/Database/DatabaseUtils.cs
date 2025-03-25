using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;

namespace Vint.Core.Utils;

public static class DatabaseUtils {
    public static async Task<Player?> GetSelfPlayerById(this DbConnection db, long userId) =>
        await GetQuery(db).SingleOrDefaultAsync(player => player.Id == userId);

    public static async Task<Player?> GetSelfPlayerByUsername(this DbConnection db, string username) =>
        await GetQuery(db).SingleOrDefaultAsync(player => player.Username == username);

    public static async Task<Player?> GetSelfPlayerByEmail(this DbConnection db, string email) =>
        await GetQuery(db).SingleOrDefaultAsync(player => player.Email == email);

    static IQueryable<Player> GetQuery(DbConnection db) =>
        db.Players
            .LoadWith(player => player.Modules)
            .LoadWith(player => player.DiscordLink);
}
