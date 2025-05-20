using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460106433434)]
public class RestorePasswordByDiscordEvent : IServerEvent {
    public string DiscordID { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        throw new NotImplementedException();
        // ILogger logger = connection.Logger.ForType<RestorePasswordByDiscordEvent>();
        // logger.Warning("Restoring password '{Id}'", DiscordID);
        //
        // if (discordBot == null ||
        //     DiscordID.Length is < 17 or > 18 ||
        //     !ulong.TryParse(DiscordID, out ulong discordId)) return;
        //
        // await using DbConnection db = new();
        //
        // Player? player = await db.Players
        //     .LoadWith(player => player.DiscordLink)
        //     .FirstOrDefaultAsync(player => player.DiscordUserId == discordId);
        //
        // DiscordLink? discordLink = player?.DiscordLink;
        //
        // if (player == null || discordLink == null) return;
        //
        // (DiscordRestClient? client, bool? isAuthorized) = await discordLink.GetClient(connection, discordBot);
        //
        // if (isAuthorized != true || client == null) {
        //     await connection.Send(new DiscordInvalidEvent(DiscordID));
        //     return;
        // }
        //
        // DiscordMember? member = await discordBot.AddToOrGetFromGuild(client.CurrentUser, player.Username, discordLink.AccessToken);
        //
        // if (member! == null!) return;
        //
        // byte[] codeBytes = new byte[4];
        // Random.Shared.NextBytes(codeBytes);
        // string code = Convert.ToHexString(codeBytes);
        //
        // connection.RestorePasswordCode = code;
        // connection.Player = player;
        // await connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(client.CurrentUser.Username));
    }
}
