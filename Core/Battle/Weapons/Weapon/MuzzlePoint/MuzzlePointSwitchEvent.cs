using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Weapon.MuzzlePoint;

[ProtocolId(-2650671245931951659)]
public class MuzzlePointSwitchEvent : MuzzlePointEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        IEntity weaponEntity = entities.Single();

        if (tanker?.Tank.WeaponHandler is not TwinsWeaponHandler twins || twins.BattleEntity != weaponEntity)
            return;

        await tanker.Round.Players
            .Where(player => player != tanker)
            .Send(new RemoteMuzzlePointSwitchEvent { Index = Index }, weaponEntity);
    }
}
