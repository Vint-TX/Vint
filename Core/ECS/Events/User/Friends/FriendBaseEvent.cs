using System.Diagnostics.CodeAnalysis;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343409998)]
public abstract class FriendBaseEvent : IEvent {
    public long User { get; protected set; }

    [field: AllowNull, MaybeNull]
    [ProtocolIgnore] public UserContainer UserContainer => field ??= UserRegistry.GetContainer(User);
}
