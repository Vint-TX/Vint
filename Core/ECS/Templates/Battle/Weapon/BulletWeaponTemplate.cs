using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

public abstract class BulletWeaponTemplate : DiscreteWeaponTemplate {
    protected override IEntity Create(string configPath, IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = base.Create(configPath, tank, battlePlayer);

        entity.AddComponent<WeaponBulletShotComponent>(configPath);
        return entity;
    }
}