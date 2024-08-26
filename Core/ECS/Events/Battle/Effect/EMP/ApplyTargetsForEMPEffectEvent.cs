using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Effect.EMP;

[ProtocolId(636250863918020313)]
public class ApplyTargetsForEMPEffectEvent : IServerEvent {
    public long[] Targets { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity emp = entities.Single();
        BattleTank? tank = connection.BattlePlayer?.Tank;
        Battles.Battle battle = tank?.Battle!;
        EMPEffect? effect = tank?.Effects
            .OfType<EMPEffect>()
            .SingleOrDefault(effect => effect.Entity == emp);

        await ChatUtils.SendMessage($"Targets: {Targets.Length}; Effect: {effect?.ToString() ?? "null"}", ChatUtils.GetChat(connection), [connection], null);

        if (tank == null || effect == null)
            return;

        BattleTank[] tanks = battle.Players
            .Select(player => player.Tank)
            .Where(targetTank => targetTank != null && Targets.Contains(targetTank.Tank.Id))
            .ToArray()!;

        await effect.Apply(tanks);
    }
}
