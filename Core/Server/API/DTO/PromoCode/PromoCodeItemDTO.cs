using EmbedIO;
using Vint.Core.Database.Models;

namespace Vint.Core.Server.API.DTO.PromoCode;

public record PromoCodeItemDTO(
    long Id,
    int Quantity
) {
    /// <summary>
    /// Validates the DTO
    /// </summary>
    /// <exception cref="HttpException">400 Bad Request</exception>
    /// <remarks>Does not validate the <see cref="Id"/> field</remarks>
    public void AssertValid() {
        if (Quantity <= 0)
            throw HttpException.BadRequest($"Quantity must be greater than 0 (id: {Id})");
    }

    public static PromoCodeItemDTO FromItem(PromoCodeItem item) =>
        new(item.Id, item.Quantity);
}
