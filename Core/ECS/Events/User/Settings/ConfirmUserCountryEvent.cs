using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1465192871085)]
public class ConfirmUserCountryEvent : IServerEvent {
    public string CountryCode { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        IEntity user = entities.Single();
        Player player = connection.Player;

        if (!user.HasComponent<UserCountryComponent>())
            await user.AddComponent(new UserCountryComponent(CountryCode));
        else
            await user.ChangeComponent<UserCountryComponent>(component => component.CountryCode = CountryCode);

        player.CountryCode = CountryCode;

        await using DbConnection db = new();
        await db.Players
            .Where(p => p.Id == player.Id)
            .Set(p => p.CountryCode, CountryCode)
            .UpdateAsync();
    }
}
