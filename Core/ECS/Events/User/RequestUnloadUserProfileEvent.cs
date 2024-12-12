using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1451368523887)]
public class RequestUnloadUserProfileEvent : IServerEvent {
    public IEntity User { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) =>
        await connection.Unshare(User);
}
