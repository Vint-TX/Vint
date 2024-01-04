using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Tank;
using Vint.Core.ECS.Templates.Battle.Weapon;
using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Server;

namespace Vint.Core.Battles.Player;

public class BattleTank {
    public BattleTank(BattlePlayer battlePlayer) {
        BattlePlayer = battlePlayer;
        Battle = battlePlayer.Battle;

        IPlayerConnection playerConnection = battlePlayer.PlayerConnection;

        Preset preset = playerConnection.Player.CurrentPreset;

        IEntity weapon = preset.Weapon.GetUserEntity(playerConnection);
        IEntity hull = preset.Hull.GetUserEntity(playerConnection);

        Tank = new TankTemplate().Create(hull, preset.Hull, BattlePlayer.User);

        Weapon = weapon.TemplateAccessor!.Template switch {
            SmokyUserItemTemplate => new SmokyBattleItemTemplate().Create(Tank, BattlePlayer),
            _ => throw new NotImplementedException()
        };
    }

    public BattlePlayer BattlePlayer { get; }
    public Battle Battle { get; }

    public IEnumerable<IEntity> Entities => [Tank, Weapon];

    public IEntity Tank { get; }
    public IEntity Weapon { get; }
}