using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(31221)]
public class CheckDiscordEvent : IServerEvent {
    public string DiscordID { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        throw new NotImplementedException();
        // if (discordBot == null ||
        //     DiscordID.Length is < 17 or > 18 ||
        //     !ulong.TryParse(DiscordID, out ulong discordId)) {
        //     await connection.Send(new DiscordInvalidEvent(DiscordID));
        //     return;
        // }
        //
        // await using DbConnection db = new();
        //
        // if (await db.DiscordLinks.AnyAsync(dLink => dLink.UserId == discordId))
        //     await connection.Send(new DiscordOccupiedEvent(DiscordID));
        // else
        //     await connection.Send(new DiscordVacantEvent(DiscordID));
    }
}
