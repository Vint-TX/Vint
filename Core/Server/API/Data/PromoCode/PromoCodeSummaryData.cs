namespace Vint.Core.Server.API.Data.PromoCode;

public record PromoCodeSummaryData(
    long Id,
    string Code,
    int Uses,
    int MaxUses,
    bool CanBeUsed
) {
    public static PromoCodeSummaryData FromPromoCode(Database.Models.PromoCode promoCode) =>
        new(promoCode.Id,
            promoCode.Code,
            promoCode.Uses,
            promoCode.MaxUses,
            promoCode.CanBeUsed);
}
