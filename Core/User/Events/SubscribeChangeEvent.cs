using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1482844606270)]
public class SubscribeChangeEvent : IServerEvent {
    public bool Subscribed { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        connection.Player.Subscribed = Subscribed;

        if (Subscribed) await connection.UserContainer.Entity.AddComponent(new UserSubscribeComponent(true) { OwnerUserId = connection.Player.Id } );
        else await connection.UserContainer.Entity.RemoveComponent<UserSubscribeComponent>();
    }
}
