using Vint.Core.Server.API.DTO.Player;

namespace Vint.Core.Server.API.DTO.PromoCode;

public record PromoCodeDetailDTO(
    long Id,
    string Code,
    int Uses,
    int MaxUses,
    DateTimeOffset? ExpiresAt,
    bool CanBeUsed,
    PlayerSummaryDTO? OwnedPlayer,
    IEnumerable<PromoCodeItemDTO> Items
) {
    public static PromoCodeDetailDTO FromPromoCode(Database.Models.PromoCode promoCode) =>
        new(promoCode.Id,
            promoCode.Code,
            promoCode.Uses,
            promoCode.MaxUses,
            promoCode.ExpiresAt,
            promoCode.CanBeUsed,
            promoCode.OwnedPlayer == null ? null : PlayerSummaryDTO.FromPlayer(promoCode.OwnedPlayer),
            promoCode.Items.Select(PromoCodeItemDTO.FromItem));
}
