using Vint.Core.Battles.Weapons;
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

    public override async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not HammerWeaponHandler weaponHandler) return;

        await base.Execute(connection, serviceProvider, entities);
        await weaponHandler.SetCurrentCartridgeCount(weaponHandler.CurrentCartridgeCount - 1);
    }
}
