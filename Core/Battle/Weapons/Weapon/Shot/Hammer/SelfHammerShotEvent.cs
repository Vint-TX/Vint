using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot.Hammer;

[ProtocolId(-1937089974629265090)]
public class SelfHammerShotEvent : SelfShotEvent {
    public int RandomSeed { get; private set; }

    protected override RemoteHammerShotEvent RemoteEvent => new() {
        RandomSeed = RandomSeed,
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public override async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;

        if (tanker?.Tank.WeaponHandler is not HammerWeaponHandler hammer)
            return;

        await base.Execute(connection, entities);
        await hammer.SetCurrentCartridgeCount(hammer.CurrentCartridgeCount - 1);
    }
}
