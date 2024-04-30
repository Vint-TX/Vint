using DSharpPlus.Entities;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Discord;

[ProtocolId(1457935367814)]
public class RequestChangeUserDiscordEvent : IServerEvent {
    [ProtocolName("Email")] public string Username { get; set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        DiscordMember? member = connection.Server.DiscordBot?.GetMember(Username);

        if (member! == null!) return;

        Player player = connection.Player;

        player.DiscordId = member.Id;
        player.Preferences.DiscordConfirmed = false;

        connection.User.RemoveComponentIfPresent<ConfirmedUserDiscordComponent>();
        connection.User.RemoveComponentIfPresent<UnconfirmedUserDiscordComponent>();
        connection.User.AddComponent(new UnconfirmedUserDiscordComponent(member.Username));

        using DbConnection db = new();
        db.BeginTransaction();

        db.Players
            .Where(p => p.Id == player.Id)
            .Set(p => p.DiscordId, member.Id)
            .Update();

        db.PlayersPreferences
            .Where(prefs => prefs.PlayerId == player.Id)
            .Set(prefs => prefs.DiscordConfirmed, false)
            .Update();

        db.CommitTransaction();
    }
}
