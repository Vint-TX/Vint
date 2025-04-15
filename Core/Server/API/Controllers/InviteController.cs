using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Status;

namespace Vint.Core.Server.API.Controllers;

public class InviteController : IApiController {
    [MessageId(26)]
    public async Task<IClientDTO> CreateInvite(string code, ushort uses) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.FirstOrDefaultAsync(invite => invite.Code == code);

        if (invite != null)
            return ErrorDTO.BadRequest($"Invite with code {code} already exists", invite);

        invite = new Invite {
            Code = code,
            RemainingUses = uses
        };

        invite.Id = await db.InsertWithInt64IdentityAsync(invite);
        return SuccessDTO.Created(data: invite);
    }

    [MessageId(27)]
    public async Task<IClientDTO> GetInvites(int from, int count = 20) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        List<Invite> invites = await db.Invites
            .Skip(from)
            .Take(count)
            .ToListAsync();

        return SuccessDTO.Ok(invites);
    }

    [MessageId(28)]
    public async Task<IClientDTO> GetInvite(long id) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.FirstOrDefaultAsync(invite => invite.Id == id);

        if (invite == null)
            return ErrorDTO.NotFound($"Invite {id} not found");

        return SuccessDTO.Ok(invite);
    }

    [MessageId(29)]
    public async Task<IClientDTO> DeleteInvite(long id) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.FirstOrDefaultAsync(invite => invite.Id == id);

        if (invite == null)
            return ErrorDTO.NotFound($"Invite {id} not found");

        await db.DeleteAsync(invite);
        return SuccessDTO.NoContent();
    }
}
