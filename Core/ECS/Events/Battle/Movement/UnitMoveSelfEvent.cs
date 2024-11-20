using Vint.Core.Battles.Player;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Battle.Unit;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Battle.Movement;

[ProtocolId(1486036000129)]
public class UnitMoveSelfEvent : UnitMoveEvent, IServerEvent {
    UnitMoveRemoteEvent RemoteEvent => new(UnitMove);

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank)
            return;

        IEntity unit = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer!;
        BattleTank battleTank = battlePlayer.Tank!;
        Battles.Battle battle = battlePlayer.Battle;

        if (battleTank.Effects.All(effect => effect.Entity != unit))
            return;

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            await playerConnection.Send(RemoteEvent, unit);

        await unit.ChangeComponent<UnitMoveComponent>(component => component.Movement = UnitMove);
    }
}
