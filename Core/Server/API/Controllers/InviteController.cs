using EmbedIO;
using EmbedIO.WebApi;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Server.API.Attributes.Deserialization;
using Vint.Core.Server.API.Attributes.Methods;

namespace Vint.Core.Server.API.Controllers;

public class InviteController : WebApiController {
    [Post("/")]
    public async Task<Invite> CreateInvite([FromBody] InviteDTO inviteDTO) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Code == inviteDTO.Code);

        if (invite != null) {
            throw HttpException.BadRequest($"Invite with code {inviteDTO.Code} already exists", invite);
        }

        invite = new Invite {
            Code = inviteDTO.Code,
            RemainingUses = inviteDTO.Uses
        };

        invite.Id = await db.InsertWithInt64IdentityAsync(invite);
        return invite;
    }

    [Get("/")]
    public async Task<IEnumerable<Invite>> GetInvites() {
        await using DbConnection db = new();

        Invite[] invites = await db.Invites.ToArrayAsync();
        return invites;
    }

    [Get("/{id}")]
    public async Task<Invite> GetInvite(long id) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Id == id);

        if (invite == null) {
            throw HttpException.NotFound($"Invite {id} not found");
        }

        return invite;
    }

    [Delete("/{id}")]
    public async Task DeleteInvite(long id) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Id == id);

        if (invite == null) {
            throw HttpException.NotFound($"Invite {id} not found");
        }

        await db.DeleteAsync(invite);
    }
}

public record InviteDTO(
    string Code,
    ushort Uses
);
