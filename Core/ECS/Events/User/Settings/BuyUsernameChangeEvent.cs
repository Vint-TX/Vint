using LinqToDB;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1474537061794)]
public class BuyUsernameChangeEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;
    public long Price { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity user = entities.Single();
        long truePrice = ConfigManager.GetComponent<GoodsXPriceComponent>("payment/payable/changeuid").Price;
        bool success = Price == truePrice && connection.Player.Crystals >= truePrice;

        connection.Send(new CompleteBuyUsernameChangeEvent(success), user);
        if (!success) return;

        connection.SetXCrystals(connection.Player.XCrystals - truePrice);
        connection.SetUsername(Username);

        using (DbConnection db = new()) {
            db.Update(connection.Player);
        }

        connection.Share(new UsernameChangedNotificationTemplate().Create(Username, user));
        connection.Send(new ShowNotificationGroupEvent(1), user);
    }
}