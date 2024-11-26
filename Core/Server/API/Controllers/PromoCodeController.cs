using EmbedIO;
using EmbedIO.WebApi;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.API.Attributes.Deserialization;
using Vint.Core.Server.API.Attributes.Methods;
using Vint.Core.Utils;

namespace Vint.Core.Server.API.Controllers;

public class PromoCodeController : WebApiController {
    [Get("/")]
    public async Task<IEnumerable<PromoCodeListDTO>> GetPromoCodes([FromQuery] int from, [FromQuery(@default: 20)] int count) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        PromoCodeListDTO[] promoCodes = await db.PromoCodes
            .Skip(from)
            .Take(count)
            .Select(promoCode => PromoCodeListDTO.FromPromoCode(promoCode))
            .ToArrayAsync();

        return promoCodes;
    }

    [Post("/")]
    public async Task<PromoCodeDetailDTO> CreatePromoCode([FromBody] PromoCodeRequestDTO createModel) {
        createModel.AssertValid();

        await using DbConnection db = new();
        PromoCode? promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Code == createModel.Code)
            .LoadWith(promoCode => promoCode.Items)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .SingleOrDefaultAsync();

        if (promoCode != null)
            throw HttpException.BadRequest($"Promo code with code '{promoCode.Code}' already exists", PromoCodeDetailDTO.FromPromoCode(promoCode));

        Player? ownedPlayer = null;

        if (createModel.OwnedPlayerId != -1) {
            ownedPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == createModel.OwnedPlayerId);

            if (ownedPlayer == null)
                throw HttpException.BadRequest("Owned player does not exist");
        }

        promoCode = new PromoCode {
            Code = createModel.Code,
            MaxUses = createModel.MaxUses,
            OwnedPlayerId = createModel.OwnedPlayerId,
            ExpiresAt = createModel.ExpiresAt,
            OwnedPlayer = ownedPlayer
        };

        promoCode.Id = await db.InsertWithInt64IdentityAsync(promoCode);
        return PromoCodeDetailDTO.FromPromoCode(promoCode);
    }

    [Get("/{id}")]
    public async Task<PromoCodeDetailDTO> GetPromoCode(long id) {
        await using DbConnection db = new();

        PromoCode? promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .LoadWith(promoCode => promoCode.Items)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .SingleOrDefaultAsync();

        if (promoCode == null)
            throw HttpException.NotFound($"Promo code {id} not found");

        return PromoCodeDetailDTO.FromPromoCode(promoCode);
    }

    [Patch("/{id}")]
    public async Task<PromoCodeDetailDTO> PatchPromoCode(long id, [FromBody] PromoCodeRequestDTO patchModel) {
        patchModel.AssertValid();

        await using DbConnection db = new();
        bool codeOccupied = await db.PromoCodes.AnyAsync(promoCode => promoCode.Id != id &&
                                                                      promoCode.Code == patchModel.Code);

        if (codeOccupied)
            throw HttpException.BadRequest($"Code '{patchModel.Code}' is occupied");

        if (patchModel.MaxUses != -1) {
            bool maxUsesIsInvalid = await db.PromoCodes.AnyAsync(promoCode => promoCode.Id == id &&
                                                                              promoCode.Uses > patchModel.MaxUses);

            if (maxUsesIsInvalid)
                throw HttpException.BadRequest("'MaxUses' field should be greater than or equals to 'Uses' field");
        }

        if (patchModel.OwnedPlayerId != -1) {
            bool ownedPlayerExists = await db.Players.AnyAsync(player => player.Id == patchModel.OwnedPlayerId);

            if (!ownedPlayerExists)
                throw HttpException.BadRequest("Owned player does not exist");
        }

        int updatedCount = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .Set(promoCode => promoCode.Code, patchModel.Code)
            .Set(promoCode => promoCode.MaxUses, patchModel.MaxUses)
            .Set(promoCode => promoCode.OwnedPlayerId, patchModel.OwnedPlayerId)
            .Set(promoCode => promoCode.ExpiresAt, patchModel.ExpiresAt)
            .UpdateAsync();

        if (updatedCount <= 0)
            throw HttpException.NotFound($"Promo code {id} does not exist");

        PromoCode promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .LoadWith(promoCode => promoCode.Items)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .SingleAsync();

        return PromoCodeDetailDTO.FromPromoCode(promoCode);
    }

    [Delete("/{id}")]
    public async Task DeletePromoCode(long id) {
        await using DbConnection db = new();
        int deletedCount = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .DeleteAsync();

        if (deletedCount <= 0)
            throw HttpException.NotFound($"Promo code {id} does not exist");
    }

    [Patch("/{id}/items")]
    public async Task<PromoCodeDetailDTO> PatchItem(long id, [FromBody] PromoCodeItemDTO[] itemModels) {
        if (itemModels.HasDuplicatesBy(model => model.Id))
            throw HttpException.BadRequest("Collection contains duplicate items");

        foreach (PromoCodeItemDTO model in itemModels)
            model.AssertValid();

        await using DbConnection db = new();
        PromoCode? promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .SingleOrDefaultAsync();

        if (promoCode == null)
            throw HttpException.NotFound("Promo code does not exist");

        await db.BeginTransactionAsync();
        await db.PromoCodeItems
            .Where(item => item.PromoCodeId == id)
            .DeleteAsync();

        try {
            foreach (PromoCodeItemDTO model in itemModels) {
                bool entityExists = GlobalEntities.AllMarketTemplateEntities.Any(entity => entity.Id == model.Id &&
                                                                                           entity.HasComponent<MarketItemGroupComponent>());

                if (!entityExists)
                    throw HttpException.BadRequest($"Invalid item id {model.Id}");

                PromoCodeItem item = new() {
                    PromoCodeId = id,
                    Id = model.Id,
                    Quantity = model.Quantity
                };

                await db.InsertAsync(item);
                promoCode.Items.Add(item);
            }
        } catch {
            await db.RollbackTransactionAsync();
            throw;
        }

        await db.CommitTransactionAsync();
        return PromoCodeDetailDTO.FromPromoCode(promoCode);
    }
}

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

public record PromoCodeDetailDTO(
    long Id,
    string Code,
    int Uses,
    int MaxUses,
    DateTimeOffset? ExpiresAt,
    bool CanBeUsed,
    OwnedPlayerDTO? OwnedPlayer,
    IEnumerable<PromoCodeItemDTO> Items
) {
    public static PromoCodeDetailDTO FromPromoCode(PromoCode promoCode) =>
        new(promoCode.Id,
            promoCode.Code,
            promoCode.Uses,
            promoCode.MaxUses,
            promoCode.ExpiresAt,
            promoCode.CanBeUsed,
            OwnedPlayerDTO.FromPlayer(promoCode.OwnedPlayer),
            promoCode.Items.Select(PromoCodeItemDTO.FromItem));
}

public record OwnedPlayerDTO(
    long Id,
    string Username,
    ulong DiscordId
) {
    public static OwnedPlayerDTO? FromPlayer(Player? player) =>
        player == null ? null : new OwnedPlayerDTO(player.Id,
            player.Username,
            player.DiscordUserId);
}

public record PromoCodeListDTO(
    long Id,
    string Code,
    int Uses,
    int MaxUses,
    bool CanBeUsed
) {
    public static PromoCodeListDTO FromPromoCode(PromoCode promoCode) =>
        new(promoCode.Id,
            promoCode.Code,
            promoCode.Uses,
            promoCode.MaxUses,
            promoCode.CanBeUsed);
}

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
