using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Unit;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Movement;

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
