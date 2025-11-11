using System.Diagnostics.CodeAnalysis;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Async;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Items.Components;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.PromoCode;
using Vint.Core.Server.API.Data.Status;
using Vint.Core.Utils;

namespace Vint.Core.Server.API.Controllers;

public class PromoCodeController : IApiController {
    [MessageId(30)]
    public async Task<IClientDTO> GetPromoCodes(int from, int count = 20) {
        from = Math.Max(0, from);

        await using DbConnection db = new();
        List<PromoCodeSummaryData> promoCodes = await db.PromoCodes
            .Skip(from)
            .Take(count)
            .Select(promoCode => PromoCodeSummaryData.FromPromoCode(promoCode))
            .ToListAsync();

        return SuccessDTO.Ok(promoCodes);
    }

    [MessageId(31)]
    public async Task<IClientDTO> CreatePromoCode(string code, int maxUses, long ownedPlayerId, DateTimeOffset? expiresAt) {
        if (!IsRequestValid(code, maxUses, expiresAt, out ErrorDTO? errorDTO))
            return errorDTO;

        await using DbConnection db = new();
        PromoCode? promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Code == code)
            .LoadWith(promoCode => promoCode.Items)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .AsAsyncEnumerable()
            .FirstOrDefaultAsync();

        if (promoCode != null)
            return ErrorDTO.BadRequest($"Promo code with code '{promoCode.Code}' already exists", PromoCodeDetailData.FromPromoCode(promoCode));

        Player? ownedPlayer = null;

        if (ownedPlayerId != -1) {
            ownedPlayer = await db.Players.FirstOrDefaultAsync(player => player.Id == ownedPlayerId);

            if (ownedPlayer == null)
                return ErrorDTO.BadRequest($"Owned player with id {ownedPlayerId} does not exist");
        }

        promoCode = new PromoCode {
            Code = code,
            MaxUses = maxUses,
            OwnedPlayerId = ownedPlayerId,
            ExpiresAt = expiresAt,
            OwnedPlayer = ownedPlayer
        };

        promoCode.Id = await db.InsertWithInt64IdentityAsync(promoCode);
        return SuccessDTO.Created(PromoCodeDetailData.FromPromoCode(promoCode));
    }

    [MessageId(32)]
    public async Task<IClientDTO> GetPromoCode(long id) {
        await using DbConnection db = new();

        PromoCode? promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .LoadWith(promoCode => promoCode.Items)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .AsAsyncEnumerable()
            .FirstOrDefaultAsync();

        if (promoCode == null)
            return ErrorDTO.NotFound($"Promo code {id} not found");

        return SuccessDTO.Ok(PromoCodeDetailData.FromPromoCode(promoCode));
    }

    [MessageId(33)]
    public async Task<IClientDTO> PatchPromoCode(long id, string code, int maxUses, long ownedPlayerId, DateTimeOffset? expiresAt) {
        if (!IsRequestValid(code, maxUses, expiresAt, out ErrorDTO? errorDTO))
            return errorDTO;

        await using DbConnection db = new();
        bool codeOccupied = await db.PromoCodes.AnyAsync(promoCode => promoCode.Id != id &&
                                                                      promoCode.Code == code);

        if (codeOccupied)
            return ErrorDTO.BadRequest($"Code '{code}' is occupied");

        if (maxUses != -1) {
            bool maxUsesIsInvalid = await db.PromoCodes.AnyAsync(promoCode => promoCode.Id == id &&
                                                                              promoCode.Uses > maxUses);

            if (maxUsesIsInvalid)
                return ErrorDTO.BadRequest("'maxUses' field should be greater than or equals to 'uses' field");
        }

        if (ownedPlayerId != -1) {
            bool ownedPlayerExists = await db.Players.AnyAsync(player => player.Id == ownedPlayerId);

            if (!ownedPlayerExists)
                return ErrorDTO.BadRequest($"Owned player with id {ownedPlayerId} does not exist");
        }

        int updatedCount = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .Set(promoCode => promoCode.Code, code)
            .Set(promoCode => promoCode.MaxUses, maxUses)
            .Set(promoCode => promoCode.OwnedPlayerId, ownedPlayerId)
            .Set(promoCode => promoCode.ExpiresAt, expiresAt)
            .UpdateAsync();

        if (updatedCount <= 0)
            return ErrorDTO.NotFound($"Promo code {id} does not exist");

        PromoCode promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .LoadWith(promoCode => promoCode.Items)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .AsAsyncEnumerable()
            .SingleAsync();

        return SuccessDTO.Ok(PromoCodeDetailData.FromPromoCode(promoCode));
    }

    [MessageId(34)]
    public async Task<IClientDTO> DeletePromoCode(long id) {
        await using DbConnection db = new();
        int deletedCount = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .DeleteAsync();

        if (deletedCount <= 0)
            return ErrorDTO.NotFound($"Promo code {id} does not exist");

        return SuccessDTO.NoContent();
    }

    [MessageId(35)]
    public async Task<IClientDTO> PatchItems(long id, PromoCodeItemData[] itemModels) {
        if (itemModels.HasDuplicatesBy(model => model.Id))
            return ErrorDTO.BadRequest("Collection contains duplicate items");

        foreach (PromoCodeItemData model in itemModels) {
            if (!model.IsValid(out ErrorDTO? errorDTO))
                return errorDTO;
        }

        await using DbConnection db = new();
        PromoCode? promoCode = await db.PromoCodes
            .Where(promoCode => promoCode.Id == id)
            .LoadWith(promoCode => promoCode.OwnedPlayer)
            .AsAsyncEnumerable()
            .FirstOrDefaultAsync();

        if (promoCode == null)
            return ErrorDTO.NotFound($"Promo code {id} does not exist");

        List<PromoCodeItem> items = new(itemModels.Length);

        foreach (PromoCodeItemData model in itemModels) {
            bool entityExists = GlobalEntities.AllMarketTemplateEntities.Any(entity => entity.Id == model.Id &&
                                                                                       entity.HasComponent<MarketItemGroupComponent>());

            if (!entityExists)
                return ErrorDTO.BadRequest($"Invalid item id {model.Id}");

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

        return SuccessDTO.Ok(PromoCodeDetailData.FromPromoCode(promoCode));
    }

    static bool IsRequestValid(string code, int maxUses, DateTimeOffset? expiresAt, [NotNullWhen(false)] out ErrorDTO? errorDTO) {
        if (code.Length is <= 0 or > 32) {
            errorDTO = ErrorDTO.BadRequest("'code' field must be between 1 and 32 characters");
            return false;
        }

        if (expiresAt != null && expiresAt <= DateTimeOffset.UtcNow) {
            errorDTO = ErrorDTO.BadRequest("'expiresAt' field must be in the future or null");
            return false;
        }

        if (maxUses is 0 or < -1) {
            errorDTO = ErrorDTO.BadRequest("'maxUses' field must be -1 or greater than 0");
            return false;
        }

        errorDTO = null;
        return true;
    }
}
