using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot.Railgun;

[ProtocolId(4963057750170414217)]
public class SelfRailgunChargingShotEvent : RailgunChargingShotEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Tank!.WeaponHandler is not RailgunWeaponHandler)
            return;

        IEntity weapon = entities.Single();
        Battles.Battle battle = connection.BattlePlayer.Battle;

        RemoteRailgunChargingShotEvent serverEvent = new() { ClientTime = ClientTime };

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != connection.BattlePlayer)
                     .Select(player => player.PlayerConnection))
            await playerConnection.Send(serverEvent, weapon);
    }
}
