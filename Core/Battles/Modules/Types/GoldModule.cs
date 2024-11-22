using Vint.Core.Battles.Bonus;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Items;
using Vint.Core.Server.Game;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-150814762)]
public class GoldModule : ActiveBattleModule, IModuleWithoutEffect {
    const int MaxGoldsPerBattle = 16;

    public override string ConfigPath => "garage/module/upgrade/properties/gold";

    protected override bool ActivationCondition => PlayerGoldsCount > 0 &&
                                                   BonusProcessor != null! &&
                                                   BonusProcessor.GoldsDropped < MaxGoldsPerBattle &&
                                                   Battle.Timer.TotalMinutes > 1 &&
                                                   Battle.StateManager.CurrentState is Running;

    static IEntity MarketCounterEntity { get; } = GlobalEntities.GetEntity("misc", "GoldBonus");
    IEntity CounterEntity { get; set; } = null!;
    IBonusProcessor BonusProcessor => Battle.BonusProcessor!;
    BattlePlayer BattlePlayer => Tank.BattlePlayer;
    IPlayerConnection Connection => BattlePlayer.PlayerConnection;
    int PlayerGoldsCount => Connection.Player.GoldBoxItems;

    public bool IsActive => false;
    public bool CanBeDeactivated { get; set; } = true;

    public Task Deactivate() => Task.CompletedTask;

    public override Effect GetEffect() => throw new NotSupportedException();

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        bool dropped = await BonusProcessor.ForceDropBonus(BonusType.Gold, BattlePlayer);

        if (dropped) {
            await Connection.SetGoldBoxes(PlayerGoldsCount - 1);
            await CounterEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = PlayerGoldsCount);
            await Connection.Send(new GoldBonusesCountChangedEvent(PlayerGoldsCount), Connection.User);
        }
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (!ActivationCondition) await TryBlock(); // bruh
        else await TryUnblock();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        CounterEntity = MarketCounterEntity.GetUserEntity(Connection);
    }
}
