using EmbedIO;

namespace Vint.Core.Server.API.DTO.PromoCode;

public record PromoCodeRequestDTO(
    string Code,
    int MaxUses,
    long OwnedPlayerId,
    DateTimeOffset? ExpiresAt
) {
    /// <summary>
    /// Validates the DTO
    /// </summary>
    /// <exception cref="HttpException">400 Bad Request</exception>
    /// <remarks>Does not validate the <see cref="OwnedPlayerId"/> field</remarks>
    public void AssertValid() {
        if (Code.Length is <= 0 or > 32)
            throw HttpException.BadRequest("'Code' field must be between 1 and 32 characters");

        if (ExpiresAt != null && ExpiresAt <= DateTimeOffset.UtcNow)
            throw HttpException.BadRequest("'ExpiresAt' field must be in the future or null");

        if (MaxUses is 0 or < -1)
            throw HttpException.BadRequest("'MaxUses' field must be -1 or greater than 0");
    }
}