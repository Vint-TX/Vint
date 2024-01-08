using Vint.Core.Battles;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497614958932)]
public class UpdateBattleParamsEvent : IServerEvent {
    public BattleProperties Params { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InBattle) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if ((!battlePlayer.Battle.IsCustom || ((CustomHandler)battlePlayer.Battle.TypeHandler).Owner != connection) &&
            !connection.Player.IsAdmin) return;

        battlePlayer.Battle.UpdateProperties(Params);
    }
}