using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Chat;

[ProtocolId(1450950140104)]
public class ChatMessageReceivedEvent(
    string username,
    string message,
    long userId,
    string avatarId,
    bool isSystem
) : IEvent {
    public string Message { get; private set; } = message;
    public bool SystemMessage { get; private set; } = isSystem;
    public string UserUid { get; private set; } = username;
    public long UserId { get; private set; } = userId;
    public string UserAvatarId { get; private set; } = avatarId;
}