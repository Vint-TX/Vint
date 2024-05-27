using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1547616531111)]
public class ConnectToCustomLobbyEvent : IServerEvent {
    public long LobbyId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // todo rework for admins
        if (connection.InLobby) {
            await connection.Send(new EnterBattleLobbyFailedEvent(true, false), connection.User);
            return;
        }

        IBattleProcessor battleProcessor = connection.Server.BattleProcessor;
        Battles.Battle? battle = battleProcessor.FindByLobbyId(LobbyId);

        if (await ValidateAndJoin(connection, battle)) return;

        if (connection.Player.IsAdmin && LobbyId >= 0 && LobbyId < battleProcessor.BattlesCount) {
            battle = battleProcessor.FindByIndex((int)LobbyId);

            if (await ValidateAndJoin(connection, battle)) return;
        }

        await connection.Send(new CustomLobbyNotExistsEvent(), connection.User);
    }

    static async Task<bool> ValidateAndJoin(IPlayerConnection connection, Battles.Battle? battle) {
        if (battle is not { CanAddPlayers: true } || !battle.LobbyEntity.HasComponent<OpenToConnectLobbyComponent>()) {
            await connection.Send(new EnterBattleLobbyFailedEvent(false, true), connection.User);
            return false;
        }

        if (battle is not { TypeHandler: CustomHandler } && !connection.Player.IsAdmin) {
            await connection.Send(new CustomLobbyNotExistsEvent(), connection.User);
            return false;
        }

        await battle.AddPlayer(connection);
        return true;
    }
}
