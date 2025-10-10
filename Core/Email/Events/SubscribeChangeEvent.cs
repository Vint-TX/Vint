using LinqToDB;
using LinqToDB.Linq;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Email.Components;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Events;

[ProtocolId(1482844606270)]
public class SubscribeChangeEvent : IServerEvent {
    public bool Subscribed { get; set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.IsLoggedIn) return;

        string? newsletterUnsubscribeToken = connection.Player.NewsletterUnsubscribeToken;

        await using (DbConnection db = new()) {
            IUpdatable<Player> query = db.Players.Where(player => player.Id == connection.Player.Id)
                .Set(player => player.NewsletterSubscribed, Subscribed);

            if (Subscribed && connection.Player.NewsletterUnsubscribeToken == null) {
                newsletterUnsubscribeToken = Guid.NewGuid().ToString("N");
                query = query.Set(player => player.NewsletterUnsubscribeToken, newsletterUnsubscribeToken);
            }

            await query.UpdateAsync();
        }

        connection.Player.NewsletterSubscribed = Subscribed;
        connection.Player.NewsletterUnsubscribeToken = newsletterUnsubscribeToken;
        await connection.UserContainer.Entity.ChangeComponent<UserSubscribeComponent>(component => component.Subscribed = Subscribed);
    }
}
