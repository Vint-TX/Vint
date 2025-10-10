using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Events.EMP;

[ProtocolId(636250863918020313)]
public class ApplyTargetsForEMPEffectEvent : IServerEvent {
    public long[] Targets { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        BattleTank? tank = connection.LobbyPlayer?.Tanker?.Tank;

        if (tank == null)
            return;

        IEntity emp = entities.Single();

        EMPEffect? effect = tank.Effects
            .OfType<EMPEffect>()
            .SingleOrDefault(effect => effect.Entity == emp);

        if (effect == null)
            return;

        BattleTank[] tanks = tank.Round.Tankers
            .Select(tanker => tanker.Tank)
            .IntersectBy(Targets, targetTank => targetTank.Entities.Tank.Id)
            .ToArray();

        await effect.Apply(tanks);
    }
}
