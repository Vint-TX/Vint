using Vint.Core.Database;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Registration;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Invite;

[ProtocolId(1439810001590)]
public class InviteEnteredEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        string? code = connection.ClientSession.GetComponent<InviteComponent>().InviteCode;

        if (string.IsNullOrWhiteSpace(code)) {
            connection.Send(new InviteDoesNotExistEvent());
            return;
        }

        using DbConnection db = new();
        Database.Models.Invite? invite = db.Invites.SingleOrDefault(invite => invite.Code == code);

        if (invite is not { RemainingUses: > 0 }) {
            connection.Send(new InviteDoesNotExistEvent());
            return;
        }

        connection.Invite = invite;
        connection.Send(new CommenceRegistrationEvent());
    }
}