using System.Diagnostics.CodeAnalysis;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450343409998)]
public abstract class FriendBaseEvent : IEvent {
    public long UserId { get; protected set; }

    [field: AllowNull, MaybeNull]
    [ProtocolIgnore] public UserContainer UserContainer => field ??= UserRegistry.GetContainer(UserId);
}
