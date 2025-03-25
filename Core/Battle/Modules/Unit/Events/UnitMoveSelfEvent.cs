using Vint.Core.Battle.Modules.Unit.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Unit.Events;

[ProtocolId(1486036000129)]
public class UnitMoveSelfEvent : UnitMoveEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;

        if (tanker == null)
            return;

        IEntity unit = entities.Single();
        BattleTank tank = tanker.Tank;
        Round round = tanker.Round;

        if (tank.Effects.All(effect => effect.Entity != unit))
            return;

        await round.Players
            .Where(player => player != tanker)
            .Send(new UnitMoveRemoteEvent(UnitMove), unit);

        await unit.ChangeComponent<UnitMoveComponent>(component => component.Movement = UnitMove);
    }
}
