using System.Numerics;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Events.Battle.Effect.EMP;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Server.Game;

namespace Vint.Core.Battles.Effects;

public class EMPEffect : Effect {
    public EMPEffect(BattleTank tank, int level, TimeSpan duration, float radius) : base(tank, level) {
        Duration = duration;
        Radius = radius;
    }

    public float Radius { get; set; }

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new EMPEffectTemplate().Create(Tank.BattlePlayer, Duration, Radius);
        await ShareToAllPlayers();

        foreach (IPlayerConnection connection in Battle
                     .Players
                     .Where(player => player.InBattle)
                     .Select(player => player.PlayerConnection))
            await connection.Send(new EMPEffectReadyEvent(), Entity);

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }

    public async Task Apply(IEnumerable<BattleTank> targets) {
        Vector3 position = Tank.Position;

        BattleTank[] affectedTanks = targets
            .Where(target => Vector3.Distance(position, target.Position) <= Radius)
            .Where(target => target
                .Effects
                .OfType<InvulnerabilityEffect>()
                .All(invulnerability => !invulnerability.IsActive))
            .Where(target => target
                .Modules
                .OfType<INeutralizeEMPModule>()
                .All(neutralize => !neutralize.IsActive))
            .ToArray();

        foreach (BattleTank tank in affectedTanks)
            await tank.EMPLock(Duration);
    }
}
