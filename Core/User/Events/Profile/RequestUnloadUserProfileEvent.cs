using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Profile;

[ProtocolId(1451368523887)]
public class RequestUnloadUserProfileEvent : IServerEvent {
    public long User { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        await UserRegistry.GetContainer(User).UnshareFrom(connection);
}
