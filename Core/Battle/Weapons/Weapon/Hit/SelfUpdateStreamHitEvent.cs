using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(1430210549752)]
public class SelfUpdateStreamHitEvent : UpdateStreamHitEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        IEntity weaponEntity = entities.Single();

        if (tanker?.Tank.WeaponHandler is not StreamWeaponHandler streamWeapon || streamWeapon.BattleEntity != weaponEntity)
            return;

        await tanker.Round.Players
            .Where(player => player != tanker)
            .Send(new RemoteUpdateStreamHitEvent {
                StaticHit = StaticHit,
                TankHit = TankHit
            }, weaponEntity);
    }
}
