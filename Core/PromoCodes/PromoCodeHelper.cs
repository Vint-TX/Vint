using LinqToDB.Async;
using Vint.Core.Database;

namespace Vint.Core.PromoCodes;

public static class PromoCodeHelper {
    public static async Task<PromoCodeCheckResult> Check(long playerId, string code) {
        await using DbConnection db = new();
        Database.Models.PromoCode? promoCode = await db.PromoCodes.FirstOrDefaultAsync(promoCode => promoCode.Code == code);

        if (promoCode == null)
            return PromoCodeCheckResult.NotFound;

        if (!promoCode.CanBeUsedBy(playerId))
            return PromoCodeCheckResult.Owned;

        if (!promoCode.CanBeUsed)
            return PromoCodeCheckResult.Expired;

        bool isUsed = await db.PromoCodeRedemptions.AnyAsync(redemption => redemption.PromoCodeId == promoCode.Id &&
                                                                           redemption.PlayerId == playerId);

        if (isUsed)
            return PromoCodeCheckResult.Used;

        return PromoCodeCheckResult.Valid;
    }
}
