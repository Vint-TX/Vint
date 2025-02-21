using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
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

        if (tanker?.Tank.WeaponHandler is not StreamWeaponHandler)
            return;

        IEntity weaponEntity = entities.Single();
        Round round = tanker.Round;

        RemoteUpdateStreamHitEvent remoteEvent = new() {
            StaticHit = StaticHit,
            TankHit = TankHit
        };

        await round.Players
            .Where(player => player != tanker)
            .Send(remoteEvent, weaponEntity);
    }
}
