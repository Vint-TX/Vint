using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Shop.Events;

[ProtocolId(1465192871085)]
public class ConfirmUserCountryEvent : IServerEvent {
    public string CountryCode { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity user = connection.UserContainer.Entity;
        Player player = connection.Player;

        if (!user.HasComponent<UserCountryComponent>())
            await user.AddComponent(new UserCountryComponent(CountryCode) { OwnerUserId = user.Id });
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
