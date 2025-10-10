using System.Collections;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Graffiti.Templates;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Player.User.Templates;
using Vint.Core.Battle.Tank.Common.Templates;
using Vint.Core.Battle.Tank.Incarnation.Components;
using Vint.Core.Battle.Tank.Incarnation.Templates;
using Vint.Core.Battle.Weapons.Templates;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Items.Templates.Weapons.Market;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Battle.Tank;

public class TankEntities : IEnumerable<IEntity>, IDisposable {
    public TankEntities(BattleTank battleTank) {
        BattleTank = battleTank;

        Tanker tanker = BattleTank.Tanker;
        IPlayerConnection connection = tanker.Connection;
        Preset preset = connection.Player.CurrentPreset;

        IEntity weapon = preset.Weapon;
        IEntity weaponSkin = preset.WeaponSkin;
        IEntity shell = preset.Shell;

        IEntity hull = preset.Hull;
        IEntity hullSkin = preset.HullSkin;

        IEntity cover = preset.Cover;
        IEntity paint = preset.Paint;
        IEntity graffiti = preset.Graffiti;

        BattleUser = tanker.BattleUser;
        Tank = new TankTemplate().Create(hull, BattleUser);

        Weapon = weapon.TemplateAccessor!.Template switch {
            SmokyMarketItemTemplate => new SmokyBattleItemTemplate().Create(Tank, tanker),
            TwinsMarketItemTemplate => new TwinsBattleItemTemplate().Create(Tank, tanker),
            ThunderMarketItemTemplate => new ThunderBattleItemTemplate().Create(Tank, tanker),
            RailgunMarketItemTemplate => new RailgunBattleItemTemplate().Create(Tank, tanker),
            RicochetMarketItemTemplate => new RicochetBattleItemTemplate().Create(Tank, tanker),
            IsisMarketItemTemplate => new IsisBattleItemTemplate().Create(Tank, tanker),
            VulcanMarketItemTemplate => new VulcanBattleItemTemplate().Create(Tank, tanker),
            FreezeMarketItemTemplate => new FreezeBattleItemTemplate().Create(Tank, tanker),
            FlamethrowerMarketItemTemplate => new FlamethrowerBattleItemTemplate().Create(Tank, tanker),
            ShaftMarketItemTemplate => new ShaftBattleItemTemplate().Create(Tank, tanker),
            HammerMarketItemTemplate => new HammerBattleItemTemplate().Create(Tank, tanker),
            _ => throw new ArgumentOutOfRangeException()
        };

        HullSkin = new HullSkinBattleItemTemplate().Create(hullSkin, Tank);
        WeaponSkin = new WeaponSkinBattleItemTemplate().Create(weaponSkin, Tank);
        Cover = new WeaponPaintBattleItemTemplate().Create(cover, Tank);
        Paint = new TankPaintBattleItemTemplate().Create(paint, Tank);
        Graffiti = new GraffitiBattleItemTemplate().Create(graffiti, Tank);
        Shell = new ShellBattleItemTemplate().Create(shell, Tank);

        RoundUser = new RoundUserTemplate().Create(tanker, Tank);
        Incarnation = new TankIncarnationTemplate().Create(Tank, User, Team);
    }

    BattleTank BattleTank { get; }
    IEntity User => BattleTank.Tanker.Connection.UserContainer.Entity;
    IEntity? Team => BattleTank.Tanker.Connection.LobbyPlayer?.Team;

    public IEntity Incarnation { get; private set; }
    public IEntity RoundUser { get; }
    public IEntity BattleUser { get; }

    public IEntity Tank { get; }
    public IEntity Weapon { get; }

    public IEntity HullSkin { get; }
    public IEntity WeaponSkin { get; }

    public IEntity Cover { get; }
    public IEntity Paint { get; }

    public IEntity Graffiti { get; }
    public IEntity Shell { get; }

    public async Task RecreateIncarnation() {
        IEntity incarnation = Incarnation;
        Incarnation = new TankIncarnationTemplate().Create(Tank, User, Team);

        foreach (IPlayerConnection playerConnection in incarnation.SharedPlayers) {
            await playerConnection.Unshare(incarnation);
            await playerConnection.Share(Incarnation);
        }

        // We need to add ClientIncarnation component manually if the tank is a bot
        if (Tank.HasComponent<TankAutopilotComponent>())
            await Incarnation.AddComponent<TankClientIncarnationComponent>();

        incarnation.Dispose();
    }

    IEnumerable<IEntity> Entities => [Incarnation, RoundUser, BattleUser, Tank, Weapon, HullSkin, WeaponSkin, Cover, Paint, Graffiti, Shell];

    public IEnumerator<IEntity> GetEnumerator() => Entities.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose() {
        Incarnation.Dispose();
        RoundUser.Dispose();
        Tank.Dispose();
        Weapon.Dispose();
        HullSkin.Dispose();
        WeaponSkin.Dispose();
        Cover.Dispose();
        Paint.Dispose();
        Graffiti.Dispose();
        Shell.Dispose();

        GC.SuppressFinalize(this);
    }
}
