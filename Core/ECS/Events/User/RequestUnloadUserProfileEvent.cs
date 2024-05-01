using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1451368523887)]
public class RequestUnloadUserProfileEvent : IServerEvent {
    public long UserId { get; private set; }

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity? user = connection.SharedEntities.SingleOrDefault(entity => entity.Id == UserId);

        if (user == null) return Task.CompletedTask;

        connection.Unshare(user);
        return Task.CompletedTask;
    }
}
