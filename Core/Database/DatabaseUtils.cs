using LinqToDB;
using LinqToDB.Async;
using Vint.Core.Config;
using Vint.Core.Database.Models;

namespace Vint.Core.Database;

public static class DatabaseUtils {
    public static async Task<bool> IsUsernameTaken(this DbConnection db, string username) {
        if (ConfigManager.BotNicknames.Contains(username))
            return true;

        return await db.Players.AnyAsync(player => player.Username == username);
    }

    public static async Task<Player?> GetSelfPlayerById(this DbConnection db, long userId) =>
        await GetQuery(db).FirstOrDefaultAsync(player => player.Id == userId);

    public static async Task<Player?> GetSelfPlayerByUsername(this DbConnection db, string username) =>
        await GetQuery(db).FirstOrDefaultAsync(player => player.Username == username);

    public static async Task<Player?> GetSelfPlayerByEmail(this DbConnection db, string email) =>
        await GetQuery(db).FirstOrDefaultAsync(player => player.Email == email);

    static IQueryable<Player> GetQuery(DbConnection db) =>
        db.Players.LoadWith(player => player.Modules);
}
