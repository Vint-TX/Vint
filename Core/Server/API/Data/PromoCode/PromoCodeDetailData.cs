using Vint.Core.Server.API.Data.Player;

namespace Vint.Core.Server.API.Data.PromoCode;

public record PromoCodeDetailData(
    long Id,
    string Code,
    int Uses,
    int MaxUses,
    DateTimeOffset? ExpiresAt,
    bool CanBeUsed,
    PlayerSummaryData? OwnedPlayer,
    IEnumerable<PromoCodeItemData> Items
) {
    public static PromoCodeDetailData FromPromoCode(Database.Models.PromoCode promoCode) =>
        new(promoCode.Id,
            promoCode.Code,
            promoCode.Uses,
            promoCode.MaxUses,
            promoCode.ExpiresAt,
            promoCode.CanBeUsed,
            promoCode.OwnedPlayer == null ? null : PlayerSummaryData.FromPlayer(promoCode.OwnedPlayer),
            promoCode.Items.Select(PromoCodeItemData.FromItem));
}
