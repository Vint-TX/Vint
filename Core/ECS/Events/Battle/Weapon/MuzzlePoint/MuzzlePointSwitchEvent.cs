using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.MuzzlePoint;

[ProtocolId(-2650671245931951659)]
public class MuzzlePointSwitchEvent : MuzzlePointEvent, IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Tank!.WeaponHandler is not TwinsWeaponHandler) return Task.CompletedTask;

        IEntity weapon = entities.Single();
        Battles.Battle battle = connection.BattlePlayer.Battle;

        RemoteMuzzlePointSwitchEvent serverEvent = new() { Index = Index };

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != connection.BattlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(serverEvent, weapon);

        return Task.CompletedTask;
    }
}
