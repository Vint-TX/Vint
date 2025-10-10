using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.ClientSession.Components;
using Vint.Core.Entrance.Registration.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Invite.Events;

[ProtocolId(1439810001590)]
public class InviteEnteredEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.IsLoggedIn) return;

        string? code = connection.ClientSession.GetComponent<InviteComponent>().InviteCode;

        if (string.IsNullOrWhiteSpace(code)) {
            await connection.Send<InviteDoesNotExistEvent>();
            return;
        }

        DbConnection db = new();
        Database.Models.Invite? invite = await db.Invites.FirstOrDefaultAsync(invite => invite.Code == code);
        await db.DisposeAsync();

        if (invite is not { RemainingUses: > 0 }) {
            await connection.Send<InviteDoesNotExistEvent>();
            return;
        }

        connection.Invite = invite;
        await connection.Send<CommenceRegistrationEvent>();
    }
}
