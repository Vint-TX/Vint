using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1482844606270)]
public class SubscribeChangeEvent : IServerEvent {
    public bool Subscribed { get; private set; }

    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        connection.Player.Subscribed = Subscribed;

        if (Subscribed) connection.User.AddComponent(new UserSubscribeComponent(true));
        else connection.User.RemoveComponent<UserSubscribeComponent>();

        return Task.CompletedTask;
    }
}
