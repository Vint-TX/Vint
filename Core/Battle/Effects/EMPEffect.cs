using System.Numerics;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Events.Battle.Effect.EMP;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Effects;

public class EMPEffect : Effect {
    public EMPEffect(BattleTank tank, int level, TimeSpan duration, float radius) : base(tank, level) {
        Duration = duration;
        Radius = radius;
    }

    public float Radius { get; set; }

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new EMPEffectTemplate().Create(Tank.Tanker, Duration, Radius);
        await ShareToAllPlayers();
        await Round.Players.Send(new EMPEffectReadyEvent(), Entity);

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
