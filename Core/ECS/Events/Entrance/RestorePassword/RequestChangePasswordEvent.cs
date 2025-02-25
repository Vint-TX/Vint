﻿using Vint.Core.Discord;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460403525230)]
public class RequestChangePasswordEvent(
    DiscordBot? discordBot
) : IServerEvent {
    public string PasswordDigest { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.RestorePasswordCode == null ||
            !connection.RestorePasswordCodeValid ||
            discordBot == null) {
            connection.Player = null!;
            return;
        }

        connection.RestorePasswordCode = null;
        connection.RestorePasswordCodeValid = false;
        await connection.ClientSession.RemoveComponent<RestorePasswordCodeSentComponent>();

        await connection.ChangePassword(PasswordDigest);

        connection.Player = null!;
        await connection.Send(new LoginFailedEvent());
        await connection.Send(new AutoLoginFailedEvent());
    }
}
