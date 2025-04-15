using System.Diagnostics.CodeAnalysis;
using Vint.Core.Database.Models;
using Vint.Core.Server.API.Data.Status;

namespace Vint.Core.Server.API.Data.PromoCode;

public record PromoCodeItemData(
    long Id,
    int Quantity
) {
    /// <summary>
    /// Validates the DTO
    /// </summary>
    /// <remarks>Does not validate the <see cref="Id"/> field</remarks>
    public bool IsValid([NotNullWhen(false)] out ErrorDTO? errorDTO) {
        if (Quantity <= 0) {
            errorDTO = ErrorDTO.BadRequest($"Quantity must be greater than 0 (id: {Id})");
            return false;
        }

        errorDTO = null;
        return true;
    }

    public static PromoCodeItemData FromItem(PromoCodeItem item) =>
        new(item.Id, item.Quantity);
}
