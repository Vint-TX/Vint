using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(-4669704207166218448)]
public class ExitBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattle) return Task.CompletedTask;

        BattlePlayer battlePlayer = connection.BattlePlayer;
        Battles.Battle battle = battlePlayer.Battle;

        if (battlePlayer.IsSpectator || battlePlayer.InBattleAsTank)
            battle.RemovePlayer(battlePlayer);

        return Task.CompletedTask;
    }
}
