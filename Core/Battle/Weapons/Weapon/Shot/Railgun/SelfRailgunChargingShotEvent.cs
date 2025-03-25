using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Handlers.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon.Shot.Railgun;

[ProtocolId(4963057750170414217)]
public class SelfRailgunChargingShotEvent : RailgunChargingShotEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        IEntity weaponEntity = entities.Single();

        if (tanker?.Tank.WeaponHandler is not RailgunWeaponHandler railgun || railgun.BattleEntity != weaponEntity)
            return;

        await tanker.Round.Players
            .Where(player => player != tanker)
            .Send(new RemoteRailgunChargingShotEvent { ClientTime = ClientTime }, weaponEntity);
    }
}
