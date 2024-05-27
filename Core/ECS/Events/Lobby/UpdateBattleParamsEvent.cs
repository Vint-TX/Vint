using Vint.Core.Battles;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497614958932)]
public class UpdateBattleParamsEvent : IServerEvent {
    public BattleProperties Params { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if ((battle.TypeHandler is not CustomHandler customHandler ||
             battle.StateManager.CurrentState is not NotStarted and not Ended ||
             customHandler.Owner != connection) &&
            !connection.Player.IsAdmin) return;

        await battle.UpdateProperties(Params);
    }
}
