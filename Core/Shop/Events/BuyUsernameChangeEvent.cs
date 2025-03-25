using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Items.Components;
using Vint.Core.Notification.Events;
using Vint.Core.Notification.Templates;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Shop.Events;

[ProtocolId(1474537061794)]
public class BuyUsernameChangeEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;
    public long Price { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!RegexUtils.IsLoginValid(Username)) return;

        IEntity user = connection.UserContainer.Entity;

        long truePrice = ConfigManager.GetComponent<GoodsXPriceComponent>("payment/payable/changeuid").Price;
        bool success = Price == truePrice && connection.Player.Crystals >= truePrice;

        await connection.Send(new CompleteBuyUsernameChangeEvent(success), user);
        if (!success) return;

        await connection.ChangeXCrystals(-truePrice);
        await connection.SetUsername(Username);

        await connection.Share(new UsernameChangedNotificationTemplate().Create(Username, user));
        await connection.Send<ShowNotificationGroupEvent>(user);
    }
}
