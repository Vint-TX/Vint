using EmbedIO;
using EmbedIO.WebApi;
using LinqToDB;
using LinqToDB.Data;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.API.Attributes.Deserialization;
using Vint.Core.Server.API.Attributes.Methods;
using Vint.Core.Server.API.DTO.PromoCode;
using Vint.Core.Utils;

namespace Vint.Core.Server.API.Controllers;

public class PromoCodeController : WebApiController {
    [Get("/")]
    public async Task<IEnumerable<PromoCodeSummaryDTO>> GetPromoCodes([FromQuery] int from, [FromQuery(@default: 20)] int count) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        PromoCodeSummaryDTO[] promoCodes = await db.PromoCodes
            .Skip(from)
            .Take(count)
            .Select(promoCode => PromoCodeSummaryDTO.FromPromoCode(promoCode))
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
    public async Task<PromoCodeDetailDTO> PatchItems(long id, [FromBody] PromoCodeItemDTO[] itemModels) {
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

        List<PromoCodeItem> items = new(itemModels.Length);

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

            items.Add(item);
            promoCode.Items.Add(item);
        }

        await db.BeginTransactionAsync();
        await db.PromoCodeItems
            .Where(item => item.PromoCodeId == id)
            .DeleteAsync();

        await db.BulkCopyAsync(items);
        await db.CommitTransactionAsync();

        return PromoCodeDetailDTO.FromPromoCode(promoCode);
    }
}
