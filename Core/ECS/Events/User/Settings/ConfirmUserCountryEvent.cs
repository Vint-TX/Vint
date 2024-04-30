using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1465192871085)]
public class ConfirmUserCountryEvent : IServerEvent {
    public string CountryCode { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.Player.Preferences.CountryCode = CountryCode;
        connection.User.ChangeComponent<UserCountryComponent>(component => component.CountryCode = CountryCode);

        using DbConnection db = new();
        db.PlayersPreferences
            .Where(prefs => prefs.PlayerId == connection.Player.Id)
            .Set(prefs => prefs.CountryCode, CountryCode)
            .Update();
    }
}
