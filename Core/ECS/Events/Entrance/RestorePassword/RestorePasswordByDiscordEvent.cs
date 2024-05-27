using DSharpPlus;
using DSharpPlus.Entities;
using LinqToDB;
using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Validation;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460106433434)]
public class RestorePasswordByDiscordEvent : IServerEvent {
    public string DiscordID { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());
        logger.Warning("Restoring password '{Id}'", DiscordID);

        DiscordBot? discordBot = connection.Server.DiscordBot;

        if (discordBot == null ||
            DiscordID.Length is < 17 or > 18 ||
            !ulong.TryParse(DiscordID, out ulong discordId)) return;

        await using DbConnection db = new();
        Player? player = await db.Players
            .LoadWith(player => player.DiscordLink)
            .SingleOrDefaultAsync(player => player.DiscordUserId == discordId);

        DiscordLink? discordLink = player?.DiscordLink;

        if (player == null || discordLink == null) return;

        (DiscordClient? client, bool? isAuthorized) = await discordLink.GetClient(connection, discordBot);

        if (isAuthorized != true || client == null) {
            await connection.Send(new DiscordInvalidEvent(DiscordID));
            return;
        }

        DiscordMember? member = await discordBot.AddToOrGetFromGuild(client.CurrentUser, player.Username, discordLink.AccessToken);

        if (member! == null!) return;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordCode = code;
        connection.Player = player;
        await connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(client.CurrentUser.Username));
    }
}
