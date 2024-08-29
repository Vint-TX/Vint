using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class EnergyInjectionEffect(
    BattleTank tank,
    int level,
    float reloadPercent
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new EnergyInjectionEffectTemplate().Create(Tank.BattlePlayer, Duration, reloadPercent);
        await ShareToAllPlayers();

        IEntity weaponEntity = Tank.Weapon;

        await ReloadWeapon();
        await Tank.BattlePlayer.PlayerConnection.Send(new ExecuteEnergyInjectionEvent(), Entity, weaponEntity);

        await Deactivate();
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }

    async Task ReloadWeapon() {
        if (Tank.WeaponHandler is HammerWeaponHandler hammer)
            await hammer.FillMagazine();
    }
}
