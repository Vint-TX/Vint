using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Registration;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Invite;

[ProtocolId(1439810001590)]
public class InviteEnteredEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        string? code = connection.ClientSession.GetComponent<InviteComponent>()
            .InviteCode;

        if (string.IsNullOrWhiteSpace(code)) {
            await connection.Send(new InviteDoesNotExistEvent());
            return;
        }

        await using DbConnection db = new();
        Database.Models.Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Code == code);

        if (invite is not { RemainingUses: > 0 }) {
            await connection.Send(new InviteDoesNotExistEvent());
            return;
        }

        connection.Invite = invite;
        await connection.Send(new CommenceRegistrationEvent());
    }
}
