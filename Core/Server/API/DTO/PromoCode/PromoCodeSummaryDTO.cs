namespace Vint.Core.Server.API.DTO.PromoCode;

public record PromoCodeSummaryDTO(
    long Id,
    string Code,
    int Uses,
    int MaxUses,
    bool CanBeUsed
) {
    public static PromoCodeSummaryDTO FromPromoCode(Database.Models.PromoCode promoCode) =>
        new(promoCode.Id,
            promoCode.Code,
            promoCode.Uses,
            promoCode.MaxUses,
            promoCode.CanBeUsed);
}
