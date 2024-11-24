using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Utils;

public static class PromoCodeHelper {
    public static async Task<PromoCodeCheckResult> Check(long playerId, string code) {
        await using DbConnection db = new();
        Database.Models.PromoCode? promoCode = await db.PromoCodes.SingleOrDefaultAsync(promoCode => promoCode.Code == code);

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
