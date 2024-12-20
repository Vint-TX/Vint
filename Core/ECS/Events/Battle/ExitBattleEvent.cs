using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(-4669704207166218448)]
public class ExitBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattle) return;

        BattlePlayer battlePlayer = connection.BattlePlayer;
        Battles.Battle battle = battlePlayer.Battle;

        if (battlePlayer.IsSpectator ||
            battlePlayer.InBattleAsTank)
            await battle.RemovePlayer(battlePlayer);
    }
}
