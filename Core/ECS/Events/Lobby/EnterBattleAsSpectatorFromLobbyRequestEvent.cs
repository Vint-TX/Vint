using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1498554483631)]
public class EnterBattleAsSpectatorFromLobbyRequestEvent(
    IBattleProcessor battleProcessor
) : IServerEvent {
    public long BattleId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Battles.Battle? battle = battleProcessor.FindByBattleId(BattleId);

        if (connection.InLobby ||
            battle == null) return;

        await battle.AddPlayer(connection, true);
    }
}
