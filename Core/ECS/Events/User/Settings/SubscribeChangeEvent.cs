using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1482844606270)]
public class SubscribeChangeEvent : IServerEvent {
    public bool Subscribed { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.Player.Preferences.Subscribed = Subscribed;
        connection.User.ChangeComponent<UserSubscribeComponent>(component => component.Subscribed = Subscribed);

        using DbConnection db = new();
        db.PlayersPreferences
            .Where(prefs => prefs.PlayerId == connection.Player.Id)
            .Set(prefs => prefs.Subscribed, Subscribed)
            .Update();
    }
}
