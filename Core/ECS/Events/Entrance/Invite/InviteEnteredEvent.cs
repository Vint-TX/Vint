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

        if (code == "invalid") connection.Send(new InviteDoesNotExistEvent());
        else connection.Send(new CommenceRegistrationEvent());
    }
}