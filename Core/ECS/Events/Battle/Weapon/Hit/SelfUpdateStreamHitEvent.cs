using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(1430210549752)]
public class SelfUpdateStreamHitEvent : UpdateStreamHitEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Tank!.WeaponHandler is not StreamWeaponHandler)
            return;

        BattlePlayer battlePlayer = connection.BattlePlayer;
        IEntity weapon = entities.Single();
        Battles.Battle battle = battlePlayer.Battle;

        RemoteUpdateStreamHitEvent serverEvent = new() {
            StaticHit = StaticHit,
            TankHit = TankHit
        };

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            await playerConnection.Send(serverEvent, weapon);
    }
}
