﻿using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Chat;

[ProtocolId(1513067769958)]
public class PersonalChatOwnerComponent(
    params List<IEntity> chats
) : IComponent {
    public List<IEntity> Chats { get; private set; } = chats.ToList();
}
