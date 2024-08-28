using Vint.Core.Battles.Bonus;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Events.Items;
using Vint.Core.Server;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-150814762)]
public class GoldModule : ActiveBattleModule, IModuleWithoutEffect {
    const int MaxGoldsPerBattle = 16;

    public override string ConfigPath => "garage/module/upgrade/properties/gold";

    public override Effect GetEffect() => throw new NotSupportedException();

    protected override bool ActivationCondition => PlayerGoldsCount > 0 &&
                                                   BonusProcessor != null! &&
                                                   BonusProcessor.GoldsDropped < MaxGoldsPerBattle &&
                                                   Battle.Timer.TotalMinutes > 1 &&
                                                   Battle.StateManager.CurrentState is Running;

    public bool IsActive => false;
    public bool CanBeDeactivated { get; set; } = true;

    IBonusProcessor BonusProcessor => Battle.BonusProcessor!;
    BattlePlayer BattlePlayer => Tank.BattlePlayer;
    IPlayerConnection Connection => BattlePlayer.PlayerConnection;
    int PlayerGoldsCount => Connection.Player.GoldBoxItems;

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        bool dropped = await BonusProcessor.ForceDropBonus(BonusType.Gold, BattlePlayer);

        if (dropped) {
            await Connection.SetGoldBoxes(PlayerGoldsCount - 1);
            await Connection.Send(new GoldBonusesCountChangedEvent(PlayerGoldsCount), Connection.User);
        }
    }

    public Task Deactivate() => Task.CompletedTask;

    public override async Task Tick() {
        await base.Tick();

        if (!ActivationCondition) await TryBlock(); // bruh
        else await TryUnblock();
    }
}
