using Vint.Core.Battle.Bonus;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Items;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(-150814762)]
public class GoldModule : ActiveBattleModule, IModuleWithoutEffect {
    public override string ConfigPath => "garage/module/upgrade/properties/gold";

    protected override bool ActivationCondition => PlayerGoldsCount > 0 &&
                                                   BonusProcessor != null! &&
                                                   GoldProcessor.GoldsDropped < GoldProcessor.MaxGoldsPerRound &&
                                                   Round.Remaining.TotalMinutes > 1 &&
                                                   Round.StateManager.CurrentState is Running;

    static IEntity MarketCounterEntity { get; } = GlobalEntities.GetEntity("misc", "GoldBonus");
    IEntity CounterEntity { get; set; } = null!;
    IBonusProcessor BonusProcessor => Round.BonusProcessor!;
    GoldProcessor GoldProcessor => BonusProcessor.GoldProcessor;
    Tanker Tanker => Tank.Tanker;
    IPlayerConnection Connection => Tanker.Connection;
    int PlayerGoldsCount => Connection.Player.GoldBoxItems;

    public bool IsActive => false;
    public bool CanBeDeactivated { get; set; } = true;

    public Task Deactivate() => Task.CompletedTask;

    public override Effect GetEffect() => throw new NotSupportedException();

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        bool dropped = await GoldProcessor.DropRandom(Tanker); // maybe it's bad to use GoldProcessor instead of BonusProcessor

        if (dropped) {
            await Connection.SetGoldBoxes(PlayerGoldsCount - 1);
            await CounterEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = PlayerGoldsCount);
            await Connection.Send(new GoldBonusesCountChangedEvent(PlayerGoldsCount), Connection.UserContainer.Entity);
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
