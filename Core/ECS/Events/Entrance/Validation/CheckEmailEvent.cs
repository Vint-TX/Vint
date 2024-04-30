using System.Net.Mail;
using DSharpPlus.Entities;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273125139964)]
public class CheckEmailEvent : IServerEvent {
    [ProtocolName("Email")] public string DiscordUsername { get; private set; } = null!;
    public bool IncludeUnconfirmed { get; private set; } // todo idk how to handle it

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        DiscordBot? discord = connection.Server.DiscordBot;

        if (discord == null) {
            connection.Send(new DiscordVacantEvent(DiscordUsername));
            return;
        }

        DiscordMember? member = discord.GetMember(DiscordUsername);

        if (member! == null!) {
            connection.Send(new DiscordInvalidEvent(DiscordUsername));
            return;
        }

        using DbConnection db = new();
        PlayerPreferences? prefs = db.Players
            .Where(player => player.DiscordId == member.Id)
            .Select(player => player.Preferences)
            .SingleOrDefault();

        if (prefs == null)
            connection.Send(new DiscordVacantEvent(DiscordUsername));
        else
            connection.Send(new DiscordOccupiedEvent(DiscordUsername));

        /*if (prefs == null)
                connection.Send(new DiscordVacantEvent(DiscordUsername));
            else if (prefs.DiscordConfirmed)
                connection.Send(new DiscordOccupiedEvent(DiscordUsername));
            else if (IncludeUnconfirmed && !prefs.DiscordConfirmed)
                connection.Send(new DiscordVacantEvent(DiscordUsername));
            else
                connection.Send(new DiscordOccupiedEvent(DiscordUsername));*/
    }
}
