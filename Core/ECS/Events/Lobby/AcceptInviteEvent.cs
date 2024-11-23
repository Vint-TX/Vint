using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battles;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497349612322)]
public class AcceptInviteEvent : IServerEvent {
    [ProtocolName("lobbyId")] public long LobbyId { get; private set; }
    [ProtocolName("engineId")] public long EngineId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (connection.InLobby) {
            BattlePlayer battlePlayer = connection.BattlePlayer!;

            if (battlePlayer.InBattleAsTank ||
                battlePlayer.IsSpectator)
                await battlePlayer.Battle.RemovePlayer(battlePlayer);

            await battlePlayer.Battle.RemovePlayerFromLobby(battlePlayer);
        }

        IBattleProcessor battleProcessor = serviceProvider.GetRequiredService<IBattleProcessor>();
        Battles.Battle? battle = battleProcessor.FindByLobbyId(LobbyId);

        if (battle != null)
            await battle.AddPlayer(connection);
    }
}
