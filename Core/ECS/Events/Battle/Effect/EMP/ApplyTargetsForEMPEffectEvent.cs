using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Effect.EMP;

[ProtocolId(636250863918020313)]
public class ApplyTargetsForEMPEffectEvent : IServerEvent {
    public IEntity[] Targets { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity emp = entities.Single();
        BattleTank? tank = connection.BattlePlayer?.Tank;
        Battles.Battle battle = tank?.Battle!;

        EMPEffect? effect = tank
            ?.Effects
            .OfType<EMPEffect>()
            .SingleOrDefault(effect => effect.Entity == emp);

        if (tank == null ||
            effect == null)
            return;

        BattleTank[] tanks = battle
            .Players
            .Select(player => player.Tank)
            .Where(targetTank => Targets.Contains(targetTank!.Tank))
            .ToArray()!;

        await effect.Apply(tanks);
    }
}
